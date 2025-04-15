using OPCDAClient_GUI;

static class Program
{
    [STAThread]
    static void Main()
    {
        // 这是.NET Framework 4.7.2推荐的方式
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1());
    }
}
