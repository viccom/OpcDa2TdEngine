using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using Nett;
using Opc;
using Opc.Da;
using LiteDB;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using OpcDaSubscription;
using Serilog;

namespace OpcDaClient
{
    internal class OPC_LiteDB : IDisposable
    {
        private IMqttClient _mqttClient;
        private IMqttClientOptions _mqttOptions;
        private bool _running = true;
        private bool _sync = false;
        private bool _async = true;
        private Thread _opcThread;
        private ConcurrentQueue<Dictionary<string, ItemValueResult>> _dataQueue = new ConcurrentQueue<Dictionary<string, ItemValueResult>>();
        private const int MAX_QUEUE_SIZE = 10000;
        private LiteDatabase _liteDb;
        private ILiteCollection<BsonDocument> _dataCollection;
        private readonly ILogger _log;
        private readonly string _configPath;
        private readonly string _csvPath;
        

        public OPC_LiteDB()
        {
            // 初始化路径
            _configPath = Path.Combine(AppContext.BaseDirectory, "config", "config.toml");
            _csvPath = Path.Combine(AppContext.BaseDirectory, "config", "items.csv");
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("127.0.0.1", 6883)
                .WithCredentials("admin", "123456")
                .Build();
            _mqttClient = new MqttFactory().CreateMqttClient();

            if (File.Exists("OpcDaData.db"))
            {
                File.Delete("OpcDaData.db");
            }
            _liteDb = new LiteDatabase(":memory:"); 
            _dataCollection = _liteDb.GetCollection<BsonDocument>("OpcData");
            _log = Log.ForContext("Module", "OPCDA_Sub");
        }
        

        public void Start()
        {
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                while (_running)
                {
                    try
                    {
                        await _mqttClient.ConnectAsync(_mqttOptions);
                        break;
                    }
                    catch
                    {
                        await Task.Delay(3000);
                    }
                }
            }).Wait();

            // Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在启动OPCDA_Sub线程...");
            _log.Information("正在启动OPCDA_Sub线程...");
            mqLog.Info("OPCDA_Sub", "正在启动OPCDA_Sub线程...");
            _opcThread = new Thread(() => StartOpcClient());
            _opcThread.Start();
        }

        public void Stop()
        {
            _running = false;
            if (_opcThread != null && _opcThread.IsAlive)
            {
                _log.Information("线程退出中...");
                mqLog.Info("OPCDA_Sub", "线程退出中...");
                _opcThread.Join(2000);
            }
        }

        private void StartOpcClient()
        {
            // Console.WriteLine("正在读取配置文件config.toml...");
            _log.Information("正在读取配置文件config.toml...");
            mqLog.Info("OPCDA_Sub", "正在读取配置文件config.toml...");

            if (!File.Exists(_configPath))
            {
                _log.Error($"未找到{_configPath}文件,退出OPCDA_Sub线程");
                mqLog.Error("OPCDA_Sub", $"未找到{_configPath}文件,退出OPCDA_Sub线程");
                Program.opcDaSubStatus = false;
                return;
            }
            var config = Toml.ReadFile<Config>(_configPath);
            // Console.WriteLine($"已加载配置文件: {Newtonsoft.Json.JsonConvert.SerializeObject(config)}");
            _log.Information($"已加载配置文件: {Newtonsoft.Json.JsonConvert.SerializeObject(config)}");
            mqLog.Info("OPCDA_Sub", $"已加载配置文件: {Newtonsoft.Json.JsonConvert.SerializeObject(config)}");

            if (config == null)
            {
                _log.Error("配置文件读取失败，返回null,退出OPCDA_Sub线程");
                mqLog.Error("OPCDA_Sub", "配置文件读取失败，返回null,退出OPCDA_Sub线程");
                Program.opcDaSubStatus = false;
                return;
            }
            if (config.OpcDa == null)
            {
                _log.Error("配置文件中缺少OpcDa配置节,退出OPCDA_Sub线程");
                mqLog.Error("OPCDA_Sub", "配置文件中缺少OpcDa配置节,退出OPCDA_Sub线程");
                Program.opcDaSubStatus = false;
                return;
            }
            // Console.WriteLine($"OpcDa配置节: Host={config.OpcDa.Host}, ProgID={config.OpcDa.ProgID}");
            _log.Information($"OpcDa配置节: Host={config.OpcDa.Host}, ProgID={config.OpcDa.ProgID}");
            mqLog.Info("OPCDA_Sub", $"OpcDa配置节: Host={config.OpcDa.Host}, ProgID={config.OpcDa.ProgID}");
            if (string.IsNullOrEmpty(config.OpcDa.Host) || string.IsNullOrEmpty(config.OpcDa.ProgID))
            {
                _log.Error("OpcDa配置节中缺少Host或ProgID,退出OPCDA_Sub线程");
                mqLog.Error("OPCDA_Sub", "OpcDa配置节中缺少Host或ProgID,退出OPCDA_Sub线程");
                Program.opcDaSubStatus = false;
                return;
            }
            if (!File.Exists(_csvPath))
            {
                _log.Error($"未找到{_csvPath}文件,退出OPCDA_Sub线程");
                mqLog.Error("OPCDA_Sub", $"未找到{_csvPath}文件,退出OPCDA_Sub线程");
                Program.opcDaSubStatus = false;
                return;
            }
            var items = ReadItemsFromCsv(_csvPath);
            // Console.WriteLine($"已加载{items.Length}个OPC项");
            _log.Information($"从点表读取到{items.Length}个OPC项");
            mqLog.Info("OPCDA_Sub", $"从点表读取到{items.Length}个OPC项");

            int retryInterval = 5000;
            const int maxRetryInterval = 60000;

            while (_running)
            {
                try
                {
                    URL url = new URL($"opcda://{config.OpcDa.Host}/{config.OpcDa.ProgID}");
                    OpcCom.Factory fact = new OpcCom.Factory();
                    var server = new Opc.Da.Server(fact, null);
                    server.Connect(url, new ConnectData(new System.Net.NetworkCredential()));
                    // Console.WriteLine("OPC服务器连接成功");
                    _log.Information("OPC服务器连接成功");
                    mqLog.Info("OPCDA_Sub","OPC服务器连接成功");

                    SubscriptionState groupState = new SubscriptionState();
                    groupState.Name = "Group";
                    groupState.Active = true;
                    groupState.UpdateRate = 1000;
                    groupState.Deadband = 0;
                    var group = (Subscription)server.CreateSubscription(groupState);
                    // Console.WriteLine("订阅组创建成功");
                    _log.Information("订阅组创建成功");
                    mqLog.Info("OPCDA_Sub","订阅组创建成功");

                    group.AddItems(items);
                    // Console.WriteLine("OPC项添加成功");
                    _log.Information($"点表文件有效OPC_item {items.Length} 个，OPC组成功注册 {group.Items.Length} 个OPC_item");
                    mqLog.Info("OPCDA_Sub",$"点表文件有效OPC_item {items.Length} 个，OPC组成功注册 {group.Items.Length} 个OPC_item");
                    if (group.Items.Length != items.Length)
                    {
                        _log.Error($"订阅组中存在无效OPC_item {items.Length - group.Items.Length} 个");
                        mqLog.Error("OPCDA_Sub",$"订阅组中存在无效OPC_item {items.Length - group.Items.Length} 个");
                    }

                    try
                    {
                        ItemValueResult[] syncValues = group.Read(group.Items);
                        // Console.WriteLine("同步读数据成功");
                        _log.Information($"组内一共：{group.Items.Length} 个OPC_item；同步读数据成功{syncValues.Length}条");
                        mqLog.Info("OPCDA_Sub",$"组内一共：{group.Items.Length} 条item；同步读数据成功{syncValues.Length}条");
                        _sync = true;
                        foreach (ItemValueResult value in syncValues)
                        {
                            Console.WriteLine("同步读：ITEM：{0}，value：{1}，quality：{2}, dateType：{3}", value.ItemName, value.Value, value.Quality, value.Value.GetType());
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine($"同步读数据失败: {ex.Message}");
                        _log.Error($"同步读数据失败: {ex.Message}");
                        mqLog.Error("OPCDA_Sub",$"同步读数据失败: {ex.Message}");
                        throw;
                    }

                    try
                    {
                        IRequest quest = null;
                        group.Read(group.Items, 1, this.AsyncReadComplete, out quest);
                        _async = true;
                        // Console.WriteLine("异步读数据成功");
                        _log.Information("异步读数据成功");
                        mqLog.Info("OPCDA_Sub","异步读数据成功");
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine($"异步读数据失败: {ex.Message}");
                        _log.Information($"异步读数据失败: {ex.Message}");
                        mqLog.Error("OPCDA_Sub", $"异步读数据失败: {ex.Message}");
                    }

                    if (_async)
                    {
                        group.DataChanged += new DataChangedEventHandler(this.OnTransactionCompleted);
                    }
                    else
                    {
                        // Console.WriteLine("异步读不可用");
                        _log.Error("异步读不可用");
                        mqLog.Error("OPCDA_Sub", "异步读不可用");
                    }
                    
                    while (_running)
                    {
                        Thread.Sleep(100);
                    }

                    group.DataChanged -= this.OnTransactionCompleted;
                    group.RemoveItems(group.Items);
                    server.CancelSubscription(group);
                    group.Dispose();
                    server.Disconnect();
                }
                catch (Exception ex)
                {
                    // Console.WriteLine($"连接或订阅失败，将在{retryInterval}毫秒后重试: {ex.Message}");
                    _log.Error($"连接或订阅失败，将在{retryInterval}毫秒后重试: {ex.Message}");
                    mqLog.Error("OPCDA_Sub", $"连接或订阅失败，将在{retryInterval}毫秒后重试: {ex.Message}");
                    Thread.Sleep(retryInterval);
                    retryInterval = Math.Min(retryInterval * 2, maxRetryInterval);
                }
            }
        }

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

        public void AsyncReadComplete(object requestHandle, ItemValueResult[] values)
        {
            foreach (ItemValueResult value in values)
            {
                Console.WriteLine("异步读：ITEM：{0}，value：{1}，quality：{2}", value.ItemName, value.Value, value.Quality);
            }
            // if ((int)requestHandle == 1)
            // {
            //     Console.WriteLine("事件信号句柄为{0}", requestHandle);
            // }
        }

        private void OnTransactionCompleted(object group, object hReq, ItemValueResult[] items)
        {
            // Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] 收到数据变化通知{items.GetLength(0)}条");
            _log.Information($"收到数据变化通知{items.GetLength(0)}条");
            mqLog.Info("OPCDA_Sub",$"收到数据变化通知{items.GetLength(0)}条");
            var dataMap = new Dictionary<string, ItemValueResult>();

            for (int i = 0; i < items.GetLength(0); i++)
            {
                var csvLines = File.ReadAllLines(_csvPath);
                foreach (var line in csvLines.Skip(1))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 4 && parts[3] == items[i].ItemName)
                    {
                        items[i].DiagnosticInfo = parts[2];
                        items[i].ItemPath = parts[0];
                        dataMap[parts[0]] = items[i];
                        break;
                    }
                }
            }

            //保存数据到队列
            if (_dataQueue.Count() < MAX_QUEUE_SIZE)
            {
                _dataQueue.Enqueue(dataMap);
            }

            //发布数据到MQTT
            try
            {
                var newMap = new Dictionary<string, object>();
                foreach (var kvp in dataMap)
                {
                    var newData = new Dictionary<string, object>
                    {
                        ["Value"] = kvp.Value.Value,
                        ["Timestamp"] = kvp.Value.Timestamp,
                        ["Quality"] = kvp.Value.QualitySpecified ? kvp.Value.Quality.ToString() : "Unknown",
                        ["ItemName"] = kvp.Value.ItemName
                    };
                    newMap[kvp.Key] = newData;
                }
                string mqttdata = JsonConvert.SerializeObject(newMap);
                if (_mqttClient.IsConnected)
                {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic("/opcda/data")
                        .WithPayload(mqttdata)
                        .Build();
                    _mqttClient.PublishAsync(message);
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"MQTT发布失败: {ex.Message}");
                _log.Error($"MQTT发布失败: {ex.Message}");
            }

            //保存数据到LiteDB
            SaveDataToLiteDb(dataMap);
        }

        public bool TryGetData(out Dictionary<string, ItemValueResult> data)
        {
            return _dataQueue.TryDequeue(out data);
        }

        public Dictionary<string, object> GetAllData()
        {
            var result = new Dictionary<string, object>();
            foreach (var doc in _dataCollection.FindAll())
            {
                string key = doc["_id"].AsString;
                var jsonData = new
                {
                    Value = doc["Value"].AsString,
                    Timestamp = doc["Timestamp"].RawValue,
                    Quality = doc["Quality"].AsString,
                    ItemName = doc["ItemName"].AsString
                };
                result[key] = jsonData;
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException("No data available from LiteDB");
            }

            return result;
        }

        private void SaveDataToLiteDb(Dictionary<string, ItemValueResult> dataMap)
        {
            foreach (var kvp in dataMap)
            {
                var document = new BsonDocument
                {
                    ["_id"] = kvp.Key,
                    ["Value"] = kvp.Value.Value?.ToString(),
                    ["Timestamp"] = kvp.Value.Timestamp,
                    ["Quality"] = kvp.Value.QualitySpecified ? kvp.Value.Quality.ToString() : "Unknown",
                    ["ItemName"] = kvp.Value.ItemName
                };

                _dataCollection.Upsert(document);
            }
        }

        public void Dispose()
        {
            _liteDb?.Dispose();
            _mqttClient?.DisconnectAsync().Wait();
            _mqttClient?.Dispose();
        }

        public class Config
        {
            public OpcDaConfig OpcDa { get; set; }
            public TdEngineConfig TdEngine { get; set; }
        }

        public class OpcDaConfig
        {
            public string Host { get; set; }
            public string ProgID { get; set; }
        }

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
