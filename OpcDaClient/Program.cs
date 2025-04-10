using System;
using System.Threading;
using Nancy.Hosting.Self;
using OpcDaClient;
using OpcDaSubscription; // 确保包含自定义 Bootstrapper

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
        public static OPCDA_Sub opcDaSubInstance;

        static void Main(string[] args)
        {
            var opcDaSub = new OPCDA_Sub();
            var tdEnginePub = new TdEngine_Pub(opcDaSub);
            // 赋值给静态字段，供其他模块调用
            opcDaSubInstance = opcDaSub;
            tdEnginePubInstance = tdEnginePub;
            var exitEvent = new ManualResetEvent(false);

            // 设置Ctrl+C处理
            Console.CancelKeyPress += (sender, e) => {
                opcDaSub.Stop();
                tdEnginePub.Stop();
                e.Cancel = true;
                exitEvent.Set();
            };

            // 启动服务
            opcDaSub.Start();
            opcDaSubStatus = true;
            tdEnginePub.Start();
            tdEnginePubStatus = true;

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
        }
    }
}
