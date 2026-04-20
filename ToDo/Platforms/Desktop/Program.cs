using System;
using System.Threading.Tasks;
using Uno.UI.Hosting;

namespace ToDo.Skia.Desktop;

public class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        var host = UnoPlatformHostBuilder.Create()
            .App(() => new global::ToDo.App()) 
            .UseWin32()
            .UseX11()   
            .UseMacOS() 
            .Build();

        await host.RunAsync();
    }
}

