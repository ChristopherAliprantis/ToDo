using System;
using System.Threading.Tasks;
using Uno.UI.Hosting;

namespace ToDo.Skia.Desktop;

public class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        // Unified builder for Uno 6.5+ 
        // Ensure you have the 'Uno.WinUI.Runtime.Skia.Win32' package installed!
        var host = UnoPlatformHostBuilder.Create()
            .App(() => new global::ToDo.App()) 
            .UseWin32()
            .UseX11()   // For Linux desktop
            .UseMacOS() // For macOS desktop
            .Build();

        await host.RunAsync();
    }
}

