
using Uno.Resizetizer;

namespace ToDo;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }
    public static Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }

    public static Frame? rootFrame;


    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Load WinUI Resources
        Resources.Build(r => r.Merged(
            new XamlControlsResources()));

        // Load Uno.UI.Toolkit Resources
        Resources.Build(r => r.Merged(
            new ToolkitResources()));
        var builder = this.CreateBuilder(args)
            .Configure(host => host
                .UseStorage()
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                )
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                .ConfigureServices((context, services) =>
                {
                    // TODO: Register your services
                    //services.AddSingleton<IMyService, MyService>();
                })
            );
        MainWindow = builder.Window;

        MainWindow.SetWindowIcon();
        MainWindow.Title = "ToDo";
        Host = builder.Build();
        rootFrame = MainWindow.Content as Frame;

        if (rootFrame == null)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(MainPage));
        }

        MainWindow.Activate();

    }
}
