using Microsoft.UI.Windowing;
using Uno.Resizetizer;
using Windows.UI.ViewManagement;
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

    public sealed class Themess
    {
        private static UISettings _uiSettings;
        private readonly DispatcherQueue _dispatcherQueue;
        private ThemeFileWatcher watcher;

        public event EventHandler<string> ThemeChanged;

        public Themess()
        {
            _uiSettings ??= new UISettings();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _uiSettings.ColorValuesChanged += OnColorValuesChanged;
            watcher = new ThemeFileWatcher(_dispatcherQueue, (themeText) =>
            {
                ThemeChanged?.Invoke(this, themeText);
            });
        }

        public string GetCurrentTheme()
        {
            var background = _uiSettings.GetColorValue(UIColorType.Background);
            return (background.R == 0 && background.G == 0 && background.B == 0) ? "Dark" : "Light";
        }

        private void OnColorValuesChanged(UISettings sender, object args)
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                ThemeChanged?.Invoke(this, GetCurrentTheme());
            });
        }
    }

    public class ThemeFileWatcher
    {
        private readonly FileSystemWatcher _watcher;
        private readonly DispatcherQueue _dispatcherQueue;

        public ThemeFileWatcher(DispatcherQueue dispatcherQueue, Action<string> onThemeContentChanged)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));

            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToDo"
            );

            string fileName = "theme.txt";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            _watcher = new FileSystemWatcher(folderPath, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            _watcher.Changed += async (sender, e) =>
            {
                string content = await ReadFileWithRetryAsync(e.FullPath);

                if (content != null)
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        onThemeContentChanged(content);
                    });
                }
            };

            _watcher.EnableRaisingEvents = true;
        }

        private async Task<string> ReadFileWithRetryAsync(string filePath)
        {
            int retries = 5;
            while (retries > 0)
            {
                try
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }
                catch (IOException)
                {
                    retries--;
                    await Task.Delay(100);
                }
            }
            return null;
        }
    }
    public App()
    {
            this.InitializeComponent();
    }

    public static Themess Themes = new Themess();
    public static async Task<string> LoadTheme()
    {
        string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToDo");

        Directory.CreateDirectory(folderPath);
        var contents = "";
        string filePath = Path.Combine(folderPath, "theme.txt");
        if (File.Exists(filePath))
        {
            contents = await File.ReadAllTextAsync(filePath);
            if (contents == "Dark" || contents == "Light" || contents == "System")
            {
                return contents;
            }
        }
        else
        {
            contents = "System";
            await Settings.UpdateTheme(contents);
        }
        return contents;
    }

    public static string LoadThemeMode(string tri)
    {
        if (tri == "Dark" || tri == "Light")
        {
            return tri;
        }
        if (tri == "System")
        {
            var themebin = Application.Current.RequestedTheme;
            if (themebin == ApplicationTheme.Dark)
            {
                return "Dark";
            }
            else if (themebin == ApplicationTheme.Light)
            {
                return "Light";
            }
        }
        return "";
    }
    public static string Theme { get; set; } = "";
    public static string ThemeMode { get; set; } = "";
    public static Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }

    public static Frame? rootFrame;
    public static INotificationService? NotificationService { get; private set; }
    public static Microsoft.UI.Dispatching.DispatcherQueue? MainDispatcher { get; private set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
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
        Theme = await LoadTheme();
        ThemeMode = LoadThemeMode(Theme);
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
        if (ThemeMode == "Light")
        {
            rootFrame.RequestedTheme = ElementTheme.Light;
        }
        else if (ThemeMode == "Dark")
        {
            rootFrame.RequestedTheme = ElementTheme.Dark;
        }
        MainWindow.Activate();
    }
}
