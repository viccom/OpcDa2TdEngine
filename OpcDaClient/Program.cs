using System;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
using OpcDaClient;
using OpcDaSubscription;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Protocol;

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
            var opcDaSub = new OPC_LiteDB();
            var tdEnginePub = new TdEngine_Pub(opcDaSub);
            var mattSub = new Mqtt_Sub();
            // 赋值给静态字段，供其他模块调用
            opcDaSubInstance = opcDaSub;
            tdEnginePubInstance = tdEnginePub;
            var exitEvent = new ManualResetEvent(false);

            var options = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(6883) // 监听端口
                .WithConnectionBacklog(100)    // 最大连接数.
                .WithConnectionValidator(context => // 直接使用 WithConnectionValidator（异步委托）
                {
                    // 获取客户端提交的用户名和密码
                    var username = context.Username;
                    var password = context.Password;
                    // 示例：检查用户名和密码（实际项目中应从数据库或配置中读取）
                    if (username != "admin" || password != "123456")
                    {
                        context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        context.ReasonString = "用户名或密码错误";
                        return;
                    }
                    Console.WriteLine($"Mqtt客户端已连接: {context.ClientId}");
                })
                .Build();
            var mqttServer = new MqttFactory().CreateMqttServer();
            
            try
            {
                Console.WriteLine("正在启动 MQTT Broker...");
                await mqttServer.StartAsync(options);
                Console.WriteLine("MQTT Broker 已成功启动，监听端口: 6883");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动 MQTT Broker 失败: {ex.Message}");
                return; // 如果启动失败，退出程序
            }
            
            // 修改退出事件处理程序
            Console.CancelKeyPress += (sender, e) => 
            {
                opcDaSub.Stop();
                tdEnginePub.Stop();
                mattSub.Stop();
                mqttServer.StopAsync().Wait();
                
                e.Cancel = true;
                exitEvent.Set();
            };

            // 启动服务
            opcDaSub.Start();
            opcDaSubStatus = true;
            tdEnginePub.Start();
            tdEnginePubStatus = true;
            mattSub.Start();

            // 启动NancyFX Web服务，传入自定义Bootstrapper
            var hostConfig = new HostConfiguration { UrlReservations = { CreateAutomatically = true } };
            using (var host = new NancyHost(new CustomBootstrapper(), hostConfig, new Uri("http://localhost:7890")))
            {
                host.Start();
                Console.WriteLine("NancyFX Web服务已启动，监听端口 7890");

                // 主线程等待退出信号
                exitEvent.WaitOne();
            }

            // 停止服务
            opcDaSub.Stop();
            opcDaSubStatus = false;
            tdEnginePub.Stop();
            tdEnginePubStatus = false;
            mattSub.Stop();
        }
    }
}
