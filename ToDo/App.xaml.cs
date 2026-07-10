using Uno.Resizetizer;
using Microsoft.UI.Windowing;
using Path = System.IO.Path;

namespace ToDo;

public partial class App : Application
{
#if DESKTOP || WINDOWS
    [DllImport("Assets/DLLs/WinRTapis.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Unicode)]
    public static extern void RegisterAppForToasts(
    string AppId,
    string AppName);
#endif
    public App()
    {
        this.InitializeComponent();
    }

    public static Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }

    public static Frame? rootFrame;
    public static INotificationService? NotificationService { get; private set; }
    public static Microsoft.UI.Dispatching.DispatcherQueue? MainDispatcher { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainDispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Resources.Build(r => r.Merged(new XamlControlsResources()));
        Resources.Build(r => r.Merged(new ToolkitResources()));

        var builder = this.CreateBuilder(args)
            .Configure(host => host
                .UseStorage()
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseConfiguration(configure: configBuilder =>
                    configBuilder.EmbeddedSource<App>()
                )
                .UseLocalization()
            );

#if __ANDROID__
        NotificationService = new global::ToDo.Droid.AndroidNotificationService();
#elif DESKTOP
        RegisterAppForToasts("com.christopheraliprantis.todo", "ToDo");
        NotificationService = new global::ToDo.Win32.Win32NotificationService();
        //global::ToDo.Win32.Win32NotificationService.ShowToast("Registered", "Your ToDo installation has registered with\nWindows.");
#endif



        MainWindow = builder.Window;
        MainWindow.SetWindowIcon();
        MainWindow.Title = "ToDo";
        Host = builder.Build();

        // 1. Check if the MainWindow already has a Frame initialized
        if (MainWindow.Content is not Frame rootFrame)
        {
            // 2. Create the root frame to manage page navigation
            rootFrame = new Frame();

            // 3. Place the frame inside the Window's content area
            MainWindow.Content = rootFrame;
        }

        // 4. Handle navigation if the app was launched from a fresh/closed state
        if (rootFrame.Content == null)
        {
            // Navigate directly to your starting page
            rootFrame.Navigate(typeof(Start), args);
        }

        // 5. Finally, make the window visible
        MainWindow.Activate();
    }
}
