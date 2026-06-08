using Uno.Resizetizer;
using Microsoft.UI.Windowing;
using Path = System.IO.Path;

namespace ToDo;

public partial class App : Application
{
#if DESKTOP
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
        
        rootFrame = MainWindow.Content as Frame ?? new Frame();
        MainWindow.Content = rootFrame;

        // 3. Handle click if the app was launched FROM a closed state
        if (!string.IsNullOrEmpty(args.Arguments))
        {
            HandleNotificationClick(args.Arguments);
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(MainPage));
        }

        MainWindow.Activate();
    }

    private void HandleNotificationClick(string arguments)
    {
        // Simple logic to navigate or act on the notification data
        if (arguments.Contains("id="))
        {
            // You can implement your navigation logic here
            // e.g., rootFrame?.Navigate(typeof(TaskDetailPage), arguments);
        }
    }
}
