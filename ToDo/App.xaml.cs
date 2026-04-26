using Uno.Resizetizer;

namespace ToDo;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

#if __UNO_SKIA_WIN32__
    private System.IO.FileSystemWatcher? _clickWatcher;
#endif

    public static Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }
    public static Frame? rootFrame;
    public static INotificationService NotificationService { get; private set; }
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
#elif __UNO_SKIA_WIN32__
        NotificationService = new global::ToDo.Win32.Win32NotificationService();
        StartWindowsNotificationListener();

        // --- HOUSEKEEPING: Clear old Windows Task Scheduler entries ---
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo 
            { 
                FileName = "schtasks.exe", 
                Arguments = "/Delete /TN ToDo_Notif_* /F", 
                CreateNoWindow = true 
            });
        }
        catch { }
#endif

        // --- CLEANUP: Auto-delete tasks that expired while app was closed ---
        MainDispatcher?.TryEnqueue(async () =>
        {
            await Notifications.CleanupPastTasks();
        });

        MainWindow = builder.Window;
        MainWindow.SetWindowIcon();
        MainWindow.Title = "ToDo";
        Host = builder.Build();
        rootFrame = MainWindow.Content as Frame ?? new Frame();
        MainWindow.Content = rootFrame;

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(MainPage));
        }

        MainWindow.Activate();
    }

    private void StartWindowsNotificationListener()
    {
#if __UNO_SKIA_WIN32__
        string tempPath = System.IO.Path.GetTempPath();

        _clickWatcher = new System.IO.FileSystemWatcher(tempPath, "todo_click_*.txt");
        
        _clickWatcher.Created += (s, e) =>
        {
            string id = System.IO.Path.GetFileNameWithoutExtension(e.Name)
                            .Replace("todo_click_", "");

            try { System.IO.File.Delete(e.FullPath); } catch { }

            MainDispatcher?.TryEnqueue(async () => 
            {
                await global::ToDo.Notifications.CancelNotif(id);
            });
        };

        _clickWatcher.EnableRaisingEvents = true;
#endif
    }
}
