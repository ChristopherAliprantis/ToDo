using Uno.Resizetizer;
using Microsoft.UI.Windowing;
using Path = System.IO.Path;

namespace ToDo;

public partial class App : Application
{
#if DESKTOP || WINDOWS
    public static partial class Imports
    {
        [LibraryImport("Assets/DLLs/WinRTapis.dll", StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        public static partial void RegisterAppForToasts(
        string AppId,
        string AppName);

        [LibraryImport("Assets/DLLs/WinRTapis.dll", StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsNotificationBlocked(
        string AppId);

        [LibraryImport("Assets/DLLs/WinRTapis.dll", StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsNotificationDisabled(
        string AppId);
    }
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

#if ANDROID
        NotificationService = new global::ToDo.Droid.AndroidNotificationService();
#elif DESKTOP || WINDOWS
        Imports.RegisterAppForToasts("com.christopheraliprantis.todo", "ToDo");
        NotificationService = new global::ToDo.Win32.Win32NotificationService();
        //App.NotificationService.ShowImmediate("Registered", "Your ToDo installation has registered with\nWindows.");
#endif
        MainWindow = builder.Window;
        MainWindow.SetWindowIcon();
        MainWindow.Title = "ToDo";
        Host = builder.Build();

        rootFrame = MainWindow.Content as Frame ?? new Frame();
        MainWindow.Content = rootFrame;

        // 3. Handle click if the app was launched FROM a closed state

        if (rootFrame.Content == null)
        {
#if DESKTOP || WINDOWS
            rootFrame.Navigate(typeof(Start));
#else
            rootFrame.Navigate(typeof(MainPage));
#endif
        }

        MainWindow.Activate();
    }
}
