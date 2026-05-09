using Uno.Resizetizer;
#if WIN32 || __UNO_SKIA_WIN32__ || DESKTOP
using Microsoft.Toolkit.Uwp.Notifications;
#endif

namespace ToDo;

public partial class App : Application
{
    [DllImport("Assets/DLLs/WinRTapis.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Unicode)]
    public static extern void RegisterAppForToasts(
    string AppId,
    string AppName);
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
#elif WIN32 || __UNO_SKIA_WIN32__
        /*try
        {
            // 1. Dynamic registration to avoid namespace collision at compile time
            var toastType = Type.GetType("Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat, Microsoft.Toolkit.Uwp.Notifications");
            if (toastType != null)
            {
                // Register identity
                var registerMethod = toastType.GetMethod("RegisterAppIdentifier");
                registerMethod?.Invoke(null, new object[] { "com.christopheraliprantis.todo" });

                // Set up click handler
                var onActivatedEvent = toastType.GetEvent("OnActivated");
                if (onActivatedEvent != null)
                {
                    // Note: You can also use 'dynamic' here for easier access
                    toastType.GetProperty("OnActivated")?.SetValue(null, (Action<dynamic>)(args => {
                        MainDispatcher?.TryEnqueue(() => HandleNotificationClick(args.Argument));
                    }));
                }
            }
        }
        catch {  Fallback if library isn't loaded } */
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
