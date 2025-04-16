using System.Drawing;
using System.Text;
using Photino.NET;
using Photino.NET.Server;
using System.Windows.Forms; // 添加此行
using System.Runtime.InteropServices; // 新增

namespace Photino.HelloPhotino.Vue;

//NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
// Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".
internal class Program
{
    public static bool IsDebugMode = false;

    // 新增：DPI感知设置
    [DllImport("Shcore.dll")]
    private static extern int SetProcessDpiAwareness(int value);
    // 2 = Per Monitor DPI Aware

    [STAThread]
    private static void Main(string[] args)
    {
        // 新增：设置为每监视器DPI感知
        try { SetProcessDpiAwareness(2); } catch { /* 已经设置或不支持时忽略 */ }

        PhotinoServer
            .CreateStaticFileServer(args, out var baseUrl)
            .RunAsync();

        // The appUrl is set to the local development server when in debug mode.
        // This helps with hot reloading and debugging.
        var appUrl = IsDebugMode ? "http://localhost:5173" : $"{baseUrl}/index.html";
        Console.WriteLine($"Serving Vue app at {appUrl}");

        // 获取主屏幕分辨率和DPI缩放比例，自适应窗口大小
        int minWidth = 1280;
        int minHeight = 900;
        int screenWidth = minWidth;
        int screenHeight = minHeight;
        try
        {
            screenWidth = Screen.PrimaryScreen.Bounds.Width;
            screenHeight = Screen.PrimaryScreen.Bounds.Height;
        }
        catch
        {
            // Fallback values if not running on Windows or reference missing
        }
        // 计算窗口宽高为屏幕分辨率的60%，但不小于1280x800
        int windowWidth = Math.Max((int)(screenWidth * 0.6), minWidth);
        int windowHeight = Math.Max((int)(screenHeight * 0.8), minHeight);

        // Window title declared here for visibility
        var windowTitle = "Photino.Vue Demo App";

        // Creating a new PhotinoWindow instance with the fluent API
        var window = new PhotinoWindow()
            .SetTitle(windowTitle)
            .SetUseOsDefaultSize(false)
            .SetSize(new Size(windowWidth, windowHeight))
            .Center()
            .SetResizable(true)
            .RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) =>
            {
                contentType = "text/javascript";
                return new MemoryStream(Encoding.UTF8.GetBytes(
                    @"(() =>{window.setTimeout(() => {alert(`🎉 Dynamically inserted JavaScript.`);}, 1000);})();"));
            })
            .RegisterWebMessageReceivedHandler((sender, message) =>
            {
                var window = (PhotinoWindow)sender;
                var response = $"Received message: \"{message}\"";
                window.SendWebMessage(response);
            })
            .Load(appUrl);

        window.WaitForClose();
    }
}