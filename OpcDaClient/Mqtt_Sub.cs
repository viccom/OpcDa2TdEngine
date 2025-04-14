using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nett;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using OpcDaSubscription;

namespace OpcDaClient
{
    internal class Mqtt_Sub : IDisposable
    {
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttOptions;
        private bool _running = true;
        

        public Mqtt_Sub()
        {
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("127.0.0.1", 6883)
                .Build();
            _mqttClient = new MqttFactory().CreateMqttClient();
            
            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine($"收到消息:\n" +
                                  $"主题: {e.ApplicationMessage.Topic}\n" +
                                  $"载荷: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}\n" +
                                  $"QoS: {e.ApplicationMessage.QualityOfServiceLevel}\n" +
                                  $"保留标志: {e.ApplicationMessage.Retain}");

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
                        await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("client/+/command").Build());
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] mqtt_Sub线程 已启动...");
                        break;
                    }
                    catch { await Task.Delay(3000); }
                }
            }).Wait();
        }


        private void HandleMqttMessage(MqttApplicationMessage message)
        {
            var payload = Encoding.UTF8.GetString(message.Payload);
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload);

            if (json == null || !json.ContainsKey("cmd") || !json.ContainsKey("type") || !json.ContainsKey("reqid"))
            {
                SendMqttResponse(false, "Invalid payload format", null, (string)json["reqid"], "default");
                return;
            }

            var topicParts = message.Topic.Split('/');
            if (topicParts.Length < 3 || topicParts[0] != "client" || topicParts[2] != "command")
            {
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
                    HandleGetCommand(type, reqid, clientId);
                    break;
                case "set":
                    HandleSetCommand(type, json, reqid, clientId);
                    break;
                default:
                    SendMqttResponse(false, "Unsupported command", null, reqid, clientId);
                    break;
            }
        }

        private void HandleGetCommand(string type, string reqid, string clientId)
        {
            switch (type)
            {
                case "apps":
                    var appsStatus = new
                    {
                        opcDaSub = Program.opcDaSubStatus, // 获取 OPCDA_Sub 线程状态
                        tdEnginePub = Program.tdEnginePubStatus // 获取 TdEngine_Pub 线程状态
                    };
                    SendMqttResponse(true, "success", appsStatus, reqid, clientId);
                    break;
                case "config":
                    var config = Toml.ReadFile<Config>("config.toml");
                    SendMqttResponse(true, "success", config, reqid, clientId);
                    break;
                case "tags":
                    var tags = ReadTagsFromCsv("items.csv");
                    SendMqttResponse(true, "success", tags, reqid, clientId);
                    break;
                case "rtdata":
                    var rtdata = Program.opcDaSubInstance.GetAllData();
                    SendMqttResponse(true, "success", rtdata, reqid, clientId);
                    break;
                default:
                    SendMqttResponse(false, "Unsupported type", null, reqid, clientId);
                    break;
            }
        }

        private void HandleSetCommand(string type, Dictionary<string, object> json, string reqid, string clientId)
        {
            // Implement set command handling here
            SendMqttResponse(false, "Set command not implemented", null, reqid, clientId);
        }

        private void SendMqttResponse(bool result, string message, object data, string reqid, string clientId)
        {
            var response = new
            {
                result,
                message,
                data,
                reqid
            };

            var mqttdata = JsonConvert.SerializeObject(response);
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"client/{clientId}/response")
                .WithPayload(mqttdata)
                .Build();

            _mqttClient.PublishAsync(mqttMessage);
        }

        public void Stop() => _running = false;
        public void Dispose() => _mqttClient?.DisconnectAsync().Wait();
        
        public class Config
        {
            public OpcDaConfig OpcDa { get; set; }
            public TdEngineConfig TdEngine { get; set; }
        }

        private List<string[]> ReadTagsFromCsv(string filePath)
        {
            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV文件未找到: {filePath}");
            }
            var lines = File.ReadAllLines(filePath);
            var items = new List<string[]>();
    
            // 跳过标题行(如果不需要跳过，改为从0开始)
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    // 使用更健壮的CSV解析方法
                    var parts = ParseCsvLine(lines[i]);
            
                    if (parts.Length >= 4)  // 假设需要至少4列
                    {
                        items.Add(parts);
                    }
                }
                catch (Exception ex)
                {
                    // 记录错误行但继续处理
                    Console.WriteLine($"解析行 {i+1} 时出错: {ex.Message}");
                }
            }
            return items;
        }
        private string[] ParseCsvLine(string line)
        {
            // 简单处理：拆分逗号，但保留引号内的内容
            var result = new List<string>();
            var inQuotes = false;
            var currentField = new StringBuilder();
    
            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
    
            // 添加最后一个字段
            result.Add(currentField.ToString());
    
            return result.ToArray();
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