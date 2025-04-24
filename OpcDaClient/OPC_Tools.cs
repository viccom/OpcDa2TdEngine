using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Opc;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Opc.Da;
using OpcDaSubscription;
using Serilog;
using BrowseElement = Opc.Da.BrowseElement;
using BrowsePosition = Opc.Da.BrowsePosition;
using Factory = OpcCom.Factory;
using Item = Opc.Da.Item;
using Server = Opc.Da.Server;

namespace OpcDaClient
{
    internal class OPC_Tools : IDisposable
    {
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttOptions;
        private bool _running = true;
        private readonly ILogger _log;
        private readonly IDiscovery _mDiscovery = new OpcCom.ServerEnumerator();

        
        public OPC_Tools()
        {
            // 初始化路径
            _log = Log.ForContext("Module", "OPC_Tools");
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("127.0.0.1", 6883)
                .WithCredentials("admin", "123456")
                .Build();
            _mqttClient = new MqttFactory().CreateMqttClient();
            
            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                HandleMqttMessage(e.ApplicationMessage);
                return Task.CompletedTask;
            });
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (_running)
                {
                    try
                    {
                        await _mqttClient.ConnectAsync(_mqttOptions);
                        await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("client/+/opctest").Build());
                        _log.Information("OPC_Tools线程 已启动...");
                        break;
                    }
                    catch { await Task.Delay(3000); }
                }

                // 新增：启动定时任务，每隔1秒发布状态
                Task.Run(async () =>
                {
                    while (_running)
                    {
                        await Task.Delay(5000); // 每隔5秒执行一次
                    }
                });
            }).Wait();
        }

        private void HandleMqttMessage(MqttApplicationMessage message)
        {
            var payload = Encoding.UTF8.GetString(message.Payload);
            // _log.Information($"收到消息\n Topic: {message.Topic}\n Payload:\n {payload}");
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload);
            // _log.Information("1");
            if (json == null || !json.ContainsKey("cmd") || !json.ContainsKey("type") || !json.ContainsKey("reqid"))
            {
                // _log.Information("1.1");
                SendMqttResponse(false, "Invalid payload format", null, (string)json["reqid"], "default");
                return;
            }

            var topicParts = message.Topic.Split('/');
            if (topicParts.Length < 3 || topicParts[0] != "client" || topicParts[2] != "opctest")
            {
                // _log.Information($"1.2,{message.Topic}");
                SendMqttResponse(false, "Invalid topic format", null, (string)json["reqid"], "default");
                return;
            }
            var clientId = topicParts[1];

            var cmd = (string)json["cmd"];
            var type = (string)json["type"];
            var reqid = (string)json["reqid"];
            switch (cmd)
            {
                case "get":
                    HandleGetCommand(type, json, reqid, clientId);
                    break;
                case "set":
                    HandleSetCommand(type, json, reqid, clientId);
                    break;
                default:
                    SendMqttResponse(false, "Unsupported command", null, reqid, clientId);
                    break;
            }
        }

        private void HandleGetCommand(string type, Dictionary<string, object> json, string reqid, string clientId)
        {
            
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json["payload"].ToString());
            var dataStr = JsonConvert.SerializeObject(data);
            _log.Information($"收到消息 type: {type}, clientId: {clientId}, dataStr : {dataStr}");
            mqLog.Info("OPC_Tools",$"收到消息 type: {type}, clientId: {clientId}, dataStr : {dataStr}");
            var host = data.ContainsKey("Host") ? (string)data["Host"] : "";
            var ProgID = data.ContainsKey("ProgID") ? (string)data["ProgID"] : "";
            var branch = data.ContainsKey("Branch") ? (string)data["Branch"] : "";
            string [] items = data.ContainsKey("Items") ? JsonConvert.DeserializeObject<string []>(data["Items"].ToString()) : new string[0];
            // var qtags = data.ContainsKey("tags") ? JsonConvert.DeserializeObject<List<string>>(data["tags"].ToString()) : new List<string>();
            switch (type)
            {
                case "serverList":
                    var serverList = GetServerList(host);
                    // var strServerList = JsonConvert.SerializeObject(serverList);
                    SendMqttResponse(true, "success", serverList, reqid, clientId);
                    break;
                case "serverBranch":
                    var serverBranch = GetServerBranch(host, ProgID, branch);
                    // var strServerBranch = JsonConvert.SerializeObject(serverBranch);
                    SendMqttResponse(true, "success", serverBranch, reqid, clientId);
                    break;
                    
                case "serverTags":
                    var newtags = GetServerTags(host, ProgID, branch);
                    // var strTags = JsonConvert.SerializeObject(newtags);
                    SendMqttResponse(true, "success", newtags, reqid, clientId);
                    break;
                case "syncRead":
                    var syncdata = OPCsyncRead(host, ProgID, items);
                    SendMqttResponse(true, "success", syncdata, reqid, clientId);
                    break;
                case "asyncRead":
                    OPCasyncRead(host, ProgID, items, reqid, clientId);
                    // SendMqttResponse(true, "success", asyncdata, reqid, clientId);
                    break;                    
                default:
                    SendMqttResponse(false, "Unsupported type", null, reqid, clientId);
                    break;
            }
        }

        private object GetServerList(string host)
        {
            // _log.Information($"遍历 {host} 中的 OPCDA 服务器列表: ");
            List<string> serverNames = new List<string>();
            Opc.Server[] servers = _mDiscovery.GetAvailableServers(Specification.COM_DA_20, host, null);
            if (servers != null)
            {
                foreach (Opc.Da.Server server in servers)
                {
                    serverNames.Add(server.Name);
                }
            }
            _log.Information($"GetServerList，获取{host} 中的 OPCDA 服务器列表: {string.Join("\n", serverNames)}");
            mqLog.Info("OPC_Tools", $"GetServerList，获取{host} 中的 OPCDA 服务器列表: {string.Join("\n", serverNames)}");
            return serverNames;
        }

        private object GetServerBranch(string host, string progId, string branch)
        {
            try
            {
                URL url = new URL($"opcda://{host}/{progId}");
                OpcCom.Factory fact = new OpcCom.Factory();
                var server = new Opc.Da.Server(fact, null);
                server.Connect(url, new ConnectData(new System.Net.NetworkCredential()));
                _log.Information($"GetServerBranch，OPC服务器{progId}连接成功");
                mqLog.Info("OPC_Tools", $"GetServerBranch，OPC服务器{progId}连接成功");

                BrowsePosition browsePosition = null;
                try
                {
                    // OPC DA API中没有平铺浏览选项，我们需要直接浏览根节点
                    // 注意：Browse方法的三个参数是 (itemID, filters, browsePosition)
                    // itemID为null表示从根节点开始浏览，filters用于设置筛选条件
                    // BrowseElement[] elements = server.Browse(
                    //     null,  // 从根节点开始浏览
                    //     null,  // 不使用过滤器
                    //     out browsePosition  // 这应该是ref参数，不是out参数
                    // );
                    
                    ItemIdentifier branchNode = new ItemIdentifier
                    {
                        ItemName = branch // 节点路径
                    };
                    BrowseFilters filters = new BrowseFilters() {BrowseFilter = browseFilter.branch};
                    BrowseElement[] elements = server.Browse(branchNode, filters, out _);
                    // 遍历 items 获取 item.Name 并返回字符串数组
                    List<string> branchNames = new List<string>();
                    if (elements != null)
                    {
                        foreach (var element in elements)
                        {
                            Console.WriteLine($"{element.HasChildren.ToString()}, {element.ItemName}, {element.IsItem}");
                            branchNames.Add(element.ItemName);
                        }
                    }
                    return branchNames;
                }
                finally
                {
                    // 确保断开连接
                    if (server != null)
                    {
                        server.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"获取OPC服务器{progId}标签时出错: {ex.Message}");
                mqLog.Error("OPC_Tools", $"获取OPC服务器{progId}标签时出错: {ex.Message}");
                return new List<string>();
            }
        }
        private object GetServerTags(string host, string progId, string branch)
        {
            try
            {
                URL url = new URL($"opcda://{host}/{progId}");
                OpcCom.Factory fact = new OpcCom.Factory();
                var server = new Opc.Da.Server(fact, null);
                server.Connect(url, new ConnectData(new System.Net.NetworkCredential()));
                _log.Information($"GetServerTags，OPC服务器{progId}连接成功");
                mqLog.Info("OPC_Tools", $"GetServerTags，OPC服务器{progId}连接成功");

                BrowsePosition browsePosition = null;
                try
                {
                    // OPC DA API中没有平铺浏览选项，我们需要直接浏览根节点
                    // 注意：Browse方法的三个参数是 (itemID, filters, browsePosition)
                    // itemID为null表示从根节点开始浏览，filters用于设置筛选条件
                    // BrowseElement[] elements = server.Browse(
                    //     null,  // 从根节点开始浏览
                    //     null,  // 不使用过滤器
                    //     out browsePosition  // 这应该是ref参数，不是out参数
                    // );
                    
                    ItemIdentifier branchNode = new ItemIdentifier
                    {
                        ItemName = branch // 节点路径
                    };
                    BrowsePosition position;
                    BrowseFilters filters = new BrowseFilters() {BrowseFilter = browseFilter.item};
                    BrowseElement[] elements = server.Browse(branchNode, filters, out position);
                    // 遍历 items 获取 item.Name 并返回字符串数组
                    List<string> tagNames = new List<string>();
                    if (elements != null)
                    {
                        foreach (var element in elements)
                        {
                            Console.WriteLine($"{element.HasChildren.ToString()}, {element.ItemName}, {element.IsItem}");
                            tagNames.Add(element.ItemName);
                        }
                    }
                    return tagNames;
                }
                finally
                {
                    // 确保断开连接
                    if (server != null)
                    {
                        server.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"获取OPC服务器{progId}标签时出错: {ex.Message}");
                mqLog.Error("OPC_Tools", $"获取OPC服务器{progId}标签时出错: {ex.Message}");
                return new List<string>();
            }
        }
        
        
        private object OPCsyncRead(string host, string progId, string[] items)
        {
            try
            {
                URL url = new URL($"opcda://{host}/{progId}");
                Factory fact = new Factory();
                var server = new Server(fact, null);
                server.Connect(url, new ConnectData(new NetworkCredential()));
                _log.Information($"OPC服务器{progId}连接成功");
                mqLog.Info("OPC_Tools", $"OPC服务器{progId}连接成功");
                SubscriptionState groupState = new SubscriptionState();
                    groupState.Name = "Group";
                    groupState.ClientHandle = Guid.NewGuid().ToString();
                    groupState.ServerHandle = null;
                    groupState.Active = true;
                    groupState.UpdateRate = 1000;
                    groupState.Deadband = 0;
                    var group = (Subscription)server.CreateSubscription(groupState);
                    // Console.WriteLine("订阅组创建成功");
                    _log.Information("同步读组创建成功");
                    mqLog.Info("OPC_Tools","同步读组创建成功");

                    var opcitems = new List<Item>();

                    for (int i = 0; i < items.Length; i++)
                    {
                        var item = new Item();
                        item.ItemName = items[i];
                        opcitems.Add(item);
                    }

                    group.AddItems(opcitems.ToArray());
                    if (group.Items.Length != items.Length)
                    {
                        _log.Error($"同步读组中存在无效OPC_item {items.Length - group.Items.Length} 个");
                        mqLog.Error("OPC_Tools",$"同步读组中存在无效OPC_item {items.Length - group.Items.Length} 个");
                    }

                    try
                    {
                        ItemValueResult[] syncValues = group.Read(group.Items);
                        // Console.WriteLine("同步读数据成功");
                        _log.Information($"同步读组内一共：{group.Items.Length} 个OPC_item；同步读数据成功{syncValues.Length}条");
                        mqLog.Info("OPC_Tools",$"同步读组内一共：{group.Items.Length} 条item；同步读数据成功{syncValues.Length}条");
                        var syncRdata = new List<Object[]>();
                        foreach (ItemValueResult value in syncValues)
                        {
                            Console.WriteLine("同步读：ITEM：{0}，value：{1}，quality：{2}, dateType：{3}", value.ItemName, value.Value, value.QualitySpecified.ToString(), value.Value.GetType());
                            syncRdata.Add(new[] { value.ItemName, value.Value, value.Quality.QualityBits, value.Timestamp, value.Value.GetType().Name});
                        }

                        return syncRdata;
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"同步读数据失败: {ex.Message}");
                        mqLog.Error("OPC_Tools",$"同步读数据失败: {ex.Message}");
                        throw;
                    }
                    finally
                    {
                        // 确保断开连接
                        if (server != null)
                        {
                            server.Disconnect();
                        }
                    }

            }
            catch (Exception ex)
            {
                _log.Error($"调用OPC服务器{progId}方法时出错: {ex.Message}");
                mqLog.Error("OPC_Tools", $"调用OPC服务器{progId}方法时出错: {ex.Message}");
                return new List<string>();
            }
        }
        
        private void OPCasyncRead(string host, string progId, string[] items, string reqid, string clientId)
        {
            try
            {
                URL url = new URL($"opcda://{host}/{progId}");
                Factory fact = new Factory();
                var server = new Server(fact, null);
                server.Connect(url, new ConnectData(new NetworkCredential()));
                _log.Information($"OPC服务器{progId}连接成功");
                mqLog.Info("OPC_Tools", $"OPC服务器{progId}连接成功");
                SubscriptionState groupState = new SubscriptionState();
                    groupState.Name = "Group";
                    groupState.ClientHandle = Guid.NewGuid().ToString();
                    groupState.ServerHandle = null;
                    groupState.Active = true;
                    groupState.UpdateRate = 1000;
                    groupState.Deadband = 0;
                    var group = (Subscription)server.CreateSubscription(groupState);
                    // Console.WriteLine("订阅组创建成功");
                    _log.Information("异步读组创建成功");
                    mqLog.Info("OPC_Tools","异步读组创建成功");

                    var opcitems = new List<Item>();

                    for (int i = 0; i < items.Length; i++)
                    {
                        var item = new Item();
                        item.ItemName = items[i];
                        opcitems.Add(item);
                    }

                    group.AddItems(opcitems.ToArray());
                    if (group.Items.Length != items.Length)
                    {
                        _log.Error($"异步读组中存在无效OPC_item {items.Length - group.Items.Length} 个");
                        mqLog.Error("OPC_Tools",$"异步读组中存在无效OPC_item {items.Length - group.Items.Length} 个");
                    }

                    try
                    {
                        IRequest quest = null;
                        group.Read(group.Items, 1, (requestHandle, values) => AsyncReadComplete(requestHandle, values, reqid, clientId, server), out quest);
                        _log.Information("异步读数据成功");
                        mqLog.Info("OPCDA_Sub","异步读数据成功");
                    }
                    catch (Exception ex)
                    {
                        _log.Information($"异步读数据失败: {ex.Message}");
                        mqLog.Error("OPC_Tools", $"异步读数据失败: {ex.Message}");
                    }

            }
            catch (Exception ex)
            {
                _log.Error($"调用OPC服务器{progId}方法时出错: {ex.Message}");
                mqLog.Error("OPC_Tools", $"调用OPC服务器{progId}方法时出错: {ex.Message}");
                
            }
        }

        private void HandleSetCommand(string type, Dictionary<string, object> json, string reqid, string clientId)
        {
            switch (type)
            { 
                  default:
                    SendMqttResponse(false, "Unsupported type", null, reqid, clientId);
                    break;
            }
            // Implement set command handling here
            // SendMqttResponse(false, "Set command not implemented", null, reqid, clientId);
        }

        private void SendMqttResponse(bool result, string message, object data, string reqid, string clientId)
        {
            // _log.Information($"3, 发送消息to clientId: {clientId}");
            var response = new
            {
                result,
                message,
                data,
                reqid
            };

            var mqttdata = JsonConvert.SerializeObject(response);
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"opctest/{clientId}/response")
                .WithPayload(mqttdata)
                .Build();

            _mqttClient.PublishAsync(mqttMessage);
        }
        
        public void AsyncReadComplete(object requestHandle, ItemValueResult[] values, string reqid, string clientId, Server server)
        {                
            try
            {
                var asyncRdata = new List<Object[]>();
                foreach (ItemValueResult value in values)
                {
                    asyncRdata.Add(new[] { value.ItemName, value.Value, value.Quality.QualityBits, value.Timestamp });
                }
                SendMqttResponse(true, "success", asyncRdata, reqid, clientId);
            }
            finally
            {
                Task.Run(() => server?.Disconnect()); // 异步释放
            }
        }
        public void Stop() => _running = false;
        public void Dispose() => _mqttClient?.DisconnectAsync().Wait();
     }
}