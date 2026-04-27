using Uno.Resizetizer;

namespace ToDo;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }


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
#endif
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
}
