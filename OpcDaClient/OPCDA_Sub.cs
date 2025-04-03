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
    internal class OPCDA_Sub
    {
        private bool _running = true;
        private Thread _opcThread;
        private ConcurrentQueue<Dictionary<string, ItemValueResult>> _dataQueue = new ConcurrentQueue<Dictionary<string, ItemValueResult>>();
        private const int MAX_QUEUE_SIZE = 10000;

        public void Start()
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在启动OPCDA_Sub线程...");
            _opcThread = new Thread(StartOpcClient);
            _opcThread.Start();
        }

        public void Stop()
        {
            _running = false;
        }

        private void StartOpcClient()
        {
            try
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
                URL url = new URL($"opcda://{config.OpcDa.Host}/{config.OpcDa.ProgID}");
                Opc.Da.Server server = null;
                OpcCom.Factory fact = new OpcCom.Factory();
                server = new Opc.Da.Server(fact, null);

                server.Connect(url, new ConnectData(new System.Net.NetworkCredential()));

                Subscription group;
                SubscriptionState groupState = new SubscriptionState();
                groupState.Name = "Group";
                groupState.Active = true;
                groupState.UpdateRate = 1000;
                group = (Subscription)server.CreateSubscription(groupState);

                items = group.AddItems(items);
                group.DataChanged += new DataChangedEventHandler(OnTransactionCompleted);

                while (_running)
                {
                    Thread.Sleep(100);
                }

                server.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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

        private void OnTransactionCompleted(object group, object hReq, ItemValueResult[] items)
        {
            // Console.WriteLine("------------------->");
            // Console.WriteLine("DataChanged ...");
            
            var dataMap = new Dictionary<string, ItemValueResult>();
            
            for (int i = 0; i < items.GetLength(0); i++)
            {
                //Console.WriteLine("Item DataChange -   ItemId:    {0}", items[i].ItemName);
                //Console.WriteLine("                    Value:     {0,-20}", items[i].Value);
                //Console.WriteLine("                    TimeStamp: {0:00}:{1:00}:{2:00}.{3:000}", 
                //                                        items[i].Timestamp.Hour,
                //                                        items[i].Timestamp.Minute,
                //                                        items[i].Timestamp.Second,
                //                                        items[i].Timestamp.Millisecond);
                
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
        
        public bool TryGetData(out Dictionary<string, ItemValueResult> data)
        {
            return _dataQueue.TryDequeue(out data);
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
