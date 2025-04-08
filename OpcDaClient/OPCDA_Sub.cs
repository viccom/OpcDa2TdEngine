using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using Nett;
using Opc;
using Opc.Da;

namespace OpcDaClient
{
    /// <summary>
    /// OPC DA客户端订阅类，负责与OPC服务器建立连接、订阅数据变化并处理接收到的数据
    /// </summary>
    internal class OPCDA_Sub
    {
        // 线程运行状态标志
        private bool _running = true;
        // OPC客户端线程
        private Thread _opcThread;
        // 数据队列，用于存储从OPC服务器接收到的数据
        private ConcurrentQueue<Dictionary<string, ItemValueResult>> _dataQueue = new ConcurrentQueue<Dictionary<string, ItemValueResult>>();
        // 最大队列大小，防止无限制增长导致内存溢出
        private const int MAX_QUEUE_SIZE = 10000;

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

            Console.WriteLine($"正在连接OPC服务器: opcda://{config.OpcDa.Host}/{config.OpcDa.ProgID}");
            URL url = new URL($"opcda://{config.OpcDa.Host}/{config.OpcDa.ProgID}"); try
            {
                Opc.Da.Server server = null;
                OpcCom.Factory fact = new OpcCom.Factory();
                server = new Opc.Da.Server(fact, null);

                try
                {
                    server.Connect(url, new ConnectData(new System.Net.NetworkCredential()));
                    Console.WriteLine("OPC服务器连接成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OPC服务器连接失败: {ex.Message}");
                    throw;
                }

                Subscription group;
                SubscriptionState groupState = new SubscriptionState();
                groupState.Name = "Group";
                groupState.Active = true;
                groupState.UpdateRate = 1000;
                groupState.ClientHandle = 1;
                groupState.ServerHandle = 1;
                groupState.Deadband = 0;

                try
                {
                    group = (Subscription)server.CreateSubscription(groupState);
                    Console.WriteLine("订阅组创建成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"订阅组创建失败: {ex.Message}");
                    throw;
                }

                try
                {
                    items = group.AddItems(items);
                    Console.WriteLine("OPC项添加成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OPC项添加失败: {ex.Message}");
                    throw;
                }

                //同步读取所有项的值
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

                //异步读取所有项的值
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

                try
                {
                    //注册回调事件
                    group.DataChanged += new DataChangedEventHandler(this.OnTransactionCompleted);
                    Console.WriteLine("数据变化回调注册成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"数据变化回调注册失败: {ex.Message}");
                    //throw;
                }

                while (_running)
                {
                    Thread.Sleep(100);
                }

                //取消回调事件
                group.DataChanged -= new Opc.Da.DataChangedEventHandler(this.OnTransactionCompleted);
                //移除组内item
                group.RemoveItems(group.Items);
                //结束：释放各资源
                //通知服务器要求删除组。
                server.CancelSubscription(group);
                //强制.NET资源回收站回收该subscription的所有资源。
                group.Dispose();
                //强制.NET资源回收站回收该server的所有资源。
                server.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OPC Error: {ex.Message}");
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
            Console.WriteLine("事件信号句柄为{0}", hReq);
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