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
    public static Microsoft.UI.Dispatching.DispatcherQueue? MainDispatcher { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainDispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        // 1. Setup Resources
        Resources.Build(r => r.Merged(new XamlControlsResources()));
        Resources.Build(r => r.Merged(new ToolkitResources()));

        // 2. Setup Builder
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
