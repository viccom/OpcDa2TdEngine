using System;
using System.Threading;
using OpcDaClient;

namespace OpcDaSubscription
{
    class Program
    {
        static void Main(string[] args)
        {
            var opcDaSub = new OPCDA_Sub();
            var tdEnginePub = new TdEngine_Pub(opcDaSub);
            
            // 设置Ctrl+C处理
            Console.CancelKeyPress += (sender, e) => {
                opcDaSub.Stop();
                tdEnginePub.Stop();
                e.Cancel = true;
            };

            // 启动服务
            opcDaSub.Start();
            tdEnginePub.Start();

            // 保持主线程运行
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

    }
}
