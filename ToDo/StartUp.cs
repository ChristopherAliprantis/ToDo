#if DESKTOP || WINDOWS
namespace ToDo;

public sealed partial class Start : Page
{
    public Start()
    {
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

            bool blocked = App.Imports.IsNotificationBlocked(
                "com.christopheraliprantis.todo"
            );


            bool disabled = App.Imports.IsNotificationDisabled(
                "com.christopheraliprantis.todo"
            );
            App.rootFrame.Navigate(typeof(MainPage));
        };
        
        this.SizeChanged += async(s, e) =>
        {
            splash.Height = this.ActualHeight / 2;
            splash.Width = this.ActualWidth / 4;
        };
    }
}
#endif
