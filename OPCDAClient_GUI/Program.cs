using OPCDAClient_GUI;

static class Program
{
    [STAThread]
    static void Main()
    {
        // NET Framework 4.7.2
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1());
    }
}
