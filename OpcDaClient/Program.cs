using System;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
using OpcDaClient;
using OpcDaSubscription;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Protocol;
using Serilog;

namespace OpcDaSubscription
{
    class Program
    {
        // 将状态变量定义为静态成员
        public static bool opcDaSubStatus = false;
        public static bool tdEnginePubStatus = false;
        
        // 新增：静态字段保存 tdEnginePub 实例
        public static TdEngine_Pub tdEnginePubInstance;
        // 新增：静态字段保存 OPCDA_Sub 实例
        public static OPC_LiteDB opcDaSubInstance;
        // 新增：静态字段保存 tdEnginePub 实例
        public static Mqtt_Sub MqttSubInstance;
        
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                // 修改模板，包含模块上下文
                .WriteTo.Console(outputTemplate: 
                    "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log.txt",
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Hour,
                    fileSizeLimitBytes: 50 * 1024 * 1024,
                    retainedFileCountLimit: 24)
                .CreateLogger();
            var opcDaSub = new OPC_LiteDB();
            var tdEnginePub = new TdEngine_Pub(opcDaSub);
            var mqttSub = new Mqtt_Sub();
            // 赋值给静态字段，供其他模块调用
            opcDaSubInstance = opcDaSub;
            tdEnginePubInstance = tdEnginePub;
            var exitEvent = new ManualResetEvent(false);
            
            // 修改退出事件处理程序
            Console.CancelKeyPress += (sender, e) => 
            {
                opcDaSub.Stop();
                tdEnginePub.Stop();
                mqttSub.Stop();
                e.Cancel = true;
                exitEvent.Set();
            };
            Log.ForContext("Module", "Main")
                .Information("主程序启动");
            // 启动服务
            opcDaSub.Start();
            opcDaSubStatus = true;
            tdEnginePub.Start();
            tdEnginePubStatus = true;
            mqttSub.Start();

            // 启动 NancyFX Web服务，传入自定义Bootstrapper
            var hostConfig = new HostConfiguration { UrlReservations = { CreateAutomatically = true } };
            using (var host = new NancyHost(new CustomBootstrapper(), hostConfig, new Uri("http://localhost:7890")))
            {
                host.Start();
                // Console.WriteLine("NancyFX Web服务已启动，监听端口 7890");
                Log.ForContext("Module", "NancyFX")
                    .Information("NancyFX Web服务已启动，监听端口 7890");

                // 主线程等待退出信号
                exitEvent.WaitOne();
            }

            // 停止服务
            opcDaSub.Stop();
            opcDaSubStatus = false;
            tdEnginePub.Stop();
            tdEnginePubStatus = false;
            mqttSub.Stop();
            Log.ForContext("Module", "Main")
                .Information("主程序结束");
        }
    }
}
