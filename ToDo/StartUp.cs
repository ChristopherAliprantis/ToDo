namespace ToDo;

public sealed partial class Start : Page
{
    public Start()
    {
#if DESKTOP || WINDOWS
        var sp= new Microsoft.UI.Xaml.Media.Imaging.SvgImageSource(new Uri("ms-appx:///Assets/Splash/splash_screen"));
        var splash= new Microsoft.UI.Xaml.Controls.Image
        {
            Source = sp,
            Height = 40,
            Width = 40,
            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
        };
        this.Content = splash;
        this.Loaded +=async(s,e)=>
        {
            splash.Height = this.ActualHeight / 2;
            splash.Width = this.ActualWidth / 4;

            await Task.Delay(1500);
            bool t = App.Imports.IsNotificationBlocked("com.christopheraliprantis.todo", "ToDo");
            if (t)
            {
                ContentDialog alert = new ContentDialog
                {
                    Title = "Notifications Disabled",
                    Content = "Please enable notifications in Settings, so the app's ToDos with a set time work.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                // 2. Open the dialog and capture the user's action asynchronously
                ContentDialogResult result = await alert.ShowAsync();
            }
            App.rootFrame.Navigate(typeof(MainPage));
        };
        this.SizeChanged += async(s, e) =>
        {
            splash.Height = this.ActualHeight / 2;
            splash.Width = this.ActualWidth / 4;
        };


#elif ANDROID
        App.rootFrame.Navigate(typeof(MainPage));
#endif
    }
}
