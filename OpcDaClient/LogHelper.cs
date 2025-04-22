// LogHelper.cs
using System;
using System.Collections.Concurrent;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

public static class mqLog
{
    private static readonly ConcurrentQueue<LogEntry> _queue = new ConcurrentQueue<LogEntry>();
    private static readonly IMqttClient _mqttClient;
    private static readonly Thread _consumerThread;

    static mqLog()
    {
        // 初始化MQTT客户端（实际项目需配置参数）
        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ConnectAsync(new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1", 6883)
            .WithCredentials("admin", "123456")
            .Build()).Wait();

        // 启动日志消费者线程
        _consumerThread = new Thread(ProcessLogs) { IsBackground = true };
        _consumerThread.Start();
    }

    // 核心封装方法（支持直接传参）
    public static void Enqueue(DateTime timestamp, string level, string module, string message)
    {
        _queue.Enqueue(new LogEntry
        {
            Timestamp = timestamp,
            Level = level,
            Module = module,
            Message = message
        });
    }

    // 简化版（自动填充时间戳）
    public static void Debug(string module, string message)
        => Enqueue(DateTime.Now, "Info", module, message);
    public static void Info(string module, string message)
        => Enqueue(DateTime.Now, "Info", module, message);
    
    public static void Warning(string module, string message)
        => Enqueue(DateTime.Now, "Info", module, message);
    public static void Error(string module, string message)
        => Enqueue(DateTime.Now, "Error", module, message);

    // 消费者线程处理日志
    private static void ProcessLogs()
    {
        while (true)
        {
            if (_queue.TryDequeue(out var log))
            {
                // var payload = $"[{log.Timestamp}] [{log.Level}] {log.Module}: {log.Message}";
                var payloadJson = ConvertLogToJson(log.Timestamp, log.Level, log.Module, log.Message);
                _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic("/logs/apps")
                    .WithPayload(payloadJson)
                    .Build()).Wait();
            }
            else
            {
                Thread.Sleep(100); // 队列空时休眠
            }
        }
    }
    
    public static string ConvertLogToJson(DateTime Timestamp, string level, string module, string message)
    {
        return JsonConvert.SerializeObject(new
        {
            Timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            Level = level,
            Module = module,
            Message = message
        }, Formatting.Indented);
    }
}

// 日志模型（可单独放在Models文件夹）
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Module { get; set; }
    public string Message { get; set; }
}
