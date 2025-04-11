using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using Nett;
using Opc;
using Opc.Da;
using LiteDB;

namespace OpcDaClient
{
    /// <summary>
    /// OPC DA客户端订阅类，负责与OPC服务器建立连接、订阅数据变化并处理接收到的数据
    /// </summary>
    internal class OPC_LiteDB : IDisposable
    {
        // 线程运行状态标志
        private bool _running = true;
        // OPC客户端线程
        private Thread _opcThread;
        // 数据队列，用于存储从OPC服务器接收到的数据
        private ConcurrentQueue<Dictionary<string, ItemValueResult>> _dataQueue = new ConcurrentQueue<Dictionary<string, ItemValueResult>>();
        // 最大队列大小，防止无限制增长导致内存溢出
        private const int MAX_QUEUE_SIZE = 10000;

        // 新增：LiteDB实例及数据集合
        private LiteDatabase _liteDb;
        private ILiteCollection<BsonDocument> _dataCollection;

        /// <summary>
        /// 构造函数—初始化 LiteDB
        /// </summary>
        public OPC_LiteDB()
        {
            // 新增：启动时检测并删除现有数据库文件
            if (File.Exists("OpcDaData.db"))
            {
                File.Delete("OpcDaData.db");
            }

            //_liteDb = new LiteDatabase("Filename=OpcDaData.db;Connection=shared");
            _liteDb = new LiteDatabase(":memory:"); 
            _dataCollection = _liteDb.GetCollection<BsonDocument>("OpcData");
        }

        /// <summary>
        /// 启动OPC DA客户端线程
        /// </summary>
        public void Start()
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在启动OPCDA_Sub线程...");
            _opcThread = new Thread(StartOpcClient);
            _opcThread.Start();
        }

        /// <summary>
        /// 停止OPC DA客户端线程
        /// </summary>
        public void Stop()
        {
            _running = false;
            if (_opcThread != null && _opcThread.IsAlive)
            {
                _opcThread.Join(2000);
            }
        }

        /// <summary>
        /// OPC客户端线程的主方法，负责读取配置、连接OPC服务器、订阅数据
        /// </summary>
        private void StartOpcClient()
        {
            Console.WriteLine("正在读取配置文件config.toml...");
            var config = Toml.ReadFile<Config>("config.toml");
            Console.WriteLine($"已加载配置文件: {Newtonsoft.Json.JsonConvert.SerializeObject(config)}");

            if (config == null)
            {
                throw new Exception("配置文件读取失败，返回null");
            }
            if (config.OpcDa == null)
            {
                throw new Exception("配置文件中缺少OpcDa配置节");
            }
            Console.WriteLine($"OpcDa配置节: Host={config.OpcDa.Host}, ProgID={config.OpcDa.ProgID}");
            if (string.IsNullOrEmpty(config.OpcDa.Host) || string.IsNullOrEmpty(config.OpcDa.ProgID))
            {
                throw new Exception("OpcDa配置节中缺少Host或ProgID");
            }

            var items = ReadItemsFromCsv("items.csv");
            Console.WriteLine($"已加载{items.Length}个OPC项");

            // 新增：重试间隔配置
            int retryInterval = 5000; // 初始5秒
            const int maxRetryInterval = 60000; // 最大60秒

            while (_running)
            {
                try
                {
                    // 连接服务器
                    URL url = new URL($"opcda://{config.OpcDa.Host}/{config.OpcDa.ProgID}");
                    OpcCom.Factory fact = new OpcCom.Factory();
                    var server = new Opc.Da.Server(fact, null);
                    server.Connect(url, new ConnectData(new System.Net.NetworkCredential()));
                    Console.WriteLine("OPC服务器连接成功");

                    // 创建订阅组
                    Subscription group = null;
                    SubscriptionState groupState = new SubscriptionState();
                    groupState.Name = "Group";
                    groupState.Active = true;
                    groupState.UpdateRate = 1000;
                    // groupState.ClientHandle = 1;
                    // groupState.ServerHandle = 1;
                    groupState.Deadband = 0;
                    group = (Subscription)server.CreateSubscription(groupState);
                    Console.WriteLine("订阅组创建成功");

                    // 添加项
                    items = group.AddItems(items);
                    Console.WriteLine("OPC项添加成功");

                    // 同步读取数据
                    try
                    {
                        ItemValueResult[] syncValues = group.Read(group.Items);
                        Console.WriteLine("同步读数据成功");
                        //以下遍历读到的全部值
                        foreach (ItemValueResult value in syncValues)
                        {
                            Console.WriteLine("同步读：ITEM：{0}，value：{1}，quality：{2}", value.ItemName, value.Value, value.Quality);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"同步读数据失败: {ex.Message}");
                        throw;
                    }

                    // 异步读取数据
                    try
                    {
                        IRequest quest = null;
                        group.Read(group.Items, 1, this.AsyncReadComplete, out quest);
                        Console.WriteLine("异步读数据成功");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"异步读数据失败: {ex.Message}");
                        //throw;
                    }

                    // 注册事件
                    group.DataChanged += new DataChangedEventHandler(this.OnTransactionCompleted);

                    // 进入等待循环
                    while (_running)
                    {
                        Thread.Sleep(100);
                    }

                    // 释放资源
                    group.DataChanged -= this.OnTransactionCompleted;
                    group.RemoveItems(group.Items);
                    server.CancelSubscription(group);
                    group.Dispose();
                    server.Disconnect();
                }
                catch (Exception ex)
                {
                    // 新增：异常处理与重试逻辑
                    Console.WriteLine($"连接或订阅失败，将在{retryInterval}毫秒后重试: {ex.Message}");
                    Thread.Sleep(retryInterval);
                    retryInterval = Math.Min(retryInterval * 2, maxRetryInterval); // 指数退避
                }
            }
        }

        /// <summary>
        /// 从CSV文件中读取OPC项
        /// </summary>
        /// <param name="filePath">CSV文件路径</param>
        /// <returns>Item数组</returns>
        private Item[] ReadItemsFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var items = new List<Item>();

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 4)
                {
                    var item = new Item();
                    item.ItemName = parts[3];
                    items.Add(item);
                }
            }

            return items.ToArray();
        }

        //ReadComplete回调
        public void AsyncReadComplete(object requestHandle, ItemValueResult[] values)
        {
            //Console.WriteLine("发生异步读name:{0},value:{1}", values[0].ItemName, values[0].Value);
            //以下遍历异步读到的全部值
            foreach (ItemValueResult value in values)
            {
                Console.WriteLine("异步读：ITEM：{0}，value：{1}，quality：{2}", value.ItemName, value.Value, value.Quality);
            }
            if ((int)requestHandle == 1)
            {
                Console.WriteLine("事件信号句柄为{0}", requestHandle);
            }
        }

        /// <summary>
        /// 数据变化事件处理方法，当订阅的数据发生变化时调用
        /// </summary>
        /// <param name="group">订阅组</param>
        /// <param name="hReq">请求句柄</param>
        /// <param name="items">发生变化的项</param>
        private void OnTransactionCompleted(object group, object hReq, ItemValueResult[] items)
        {
            // Console.WriteLine("------------------->");
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DataChanged ... items count: {items.GetLength(0)}");
            // Console.WriteLine("事件信号句柄为{0}", hReq);
            var dataMap = new Dictionary<string, ItemValueResult>();

            for (int i = 0; i < items.GetLength(0); i++)
            {
                // 获取csv中的点名作为key
                var csvLines = File.ReadAllLines("items.csv");
                foreach (var line in csvLines.Skip(1))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 4 && parts[3] == items[i].ItemName)
                    {
                        dataMap[parts[0]] = items[i];
                        //Console.WriteLine("点名：{0}，value：{1}，Timestamp: {2}，quality：{3}, Now: {4}", parts[0], items[i].Value, items[i].Timestamp, items[i].Quality, DateTime.Now);
                        break;
                    }
                }
            }

            // 添加到队列
            if (_dataQueue.Count < MAX_QUEUE_SIZE)
            {
                _dataQueue.Enqueue(dataMap);
            }
            else
            {
                Console.WriteLine("Warning: Data queue is full, dropping data");
            }

            // 保存数据到 LiteDB
            SaveDataToLiteDb(dataMap);

            // Console.WriteLine("-------------------<");
        }

        /// <summary>
        /// 尝试获取并移除队列中的数据
        /// </summary>
        /// <param name="data">移除的数据</param>
        /// <returns>是否成功获取数据</returns>
        public bool TryGetData(out Dictionary<string, ItemValueResult> data)
        {
            return _dataQueue.TryDequeue(out data);
        }

        /// <summary>
        /// 获取 LiteDB 中存储的数据，返回 json 结构数据
        /// </summary>
        public Dictionary<string, object> GetAllData()
        {
            var result = new Dictionary<string, object>();
            foreach (var doc in _dataCollection.FindAll())
            {
                // 获取主键 _id
                string key = doc["_id"].AsString;
                // 构造 json 结构数据
                var jsonData = new
                {
                    Value = doc["Value"].AsString,
                    Timestamp = doc["Timestamp"].RawValue, // 保留原始值 (DateTime/string)
                    Quality = doc["Quality"].AsString,
                    ItemName = doc["ItemName"].AsString
                };
                result[key] = jsonData;
            }

            // 新增：判断结果是否为空
            if (result.Count == 0)
            {
                throw new InvalidOperationException("No data available from LiteDB");
            }

            return result;
        }

        /// <summary>
        /// 保存数据到 LiteDB
        /// </summary>
        /// <param name="dataMap">数据字典</param>
        private void SaveDataToLiteDb(Dictionary<string, ItemValueResult> dataMap)
        {
            foreach (var kvp in dataMap)
            {
                var document = new BsonDocument
                {
                    ["_id"] = kvp.Key, // 将 dataMap 的 Key 作为 _id（主键）
                    ["Value"] = kvp.Value.Value?.ToString(),
                    ["Timestamp"] = kvp.Value.Timestamp,
                    ["Quality"] = kvp.Value.QualitySpecified ? kvp.Value.Quality.ToString() : "Unknown",
                    ["ItemName"] = kvp.Value.ItemName
                };

                // Upsert：自动覆盖或插入
                _dataCollection.Upsert(document);
            }
        }

        /// <summary>
        /// 释放 LiteDB 资源
        /// </summary>
        public void Dispose()
        {
            _liteDb?.Dispose();
        }

        /// <summary>
        /// 配置类，包含OPC DA和TD Engine的配置信息
        /// </summary>
        public class Config
        {
            public OpcDaConfig OpcDa { get; set; }
            public TdEngineConfig TdEngine { get; set; }
        }

        /// <summary>
        /// OPC DA配置类
        /// </summary>
        public class OpcDaConfig
        {
            public string Host { get; set; }
            public string ProgID { get; set; }
        }

        /// <summary>
        /// TD Engine配置类
        /// </summary>
        public class TdEngineConfig
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Dbname { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
