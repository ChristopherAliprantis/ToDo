namespace ToDo;

public partial class Start : Page
{
    Start()
    {
#if DESKTOP || WINDOWS
        var sp= new Microsoft.UI.Xaml.Media.Imaging.SvgImageSource(new Uri("ms-appx:///Assets/Splash/splash_screen"));
        var splash= new Microsoft.UI.Xaml.Controls.Image
        {
            Source = sp,
            Height = 40,
            Width = 40,
            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
            RenderTransform = rotationTransform,
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
        };
        this.Loaded +=(s,e)=>
        {
            splash.Height = this.ActualHeight / 2;
            splash.Width = this.ActualWidth / 4;
        };
        this.SizeChanged += (s, e) =>
        {
            splash.Height = this.ActualHeight / 2;
            splash.Width = this.ActualWidth / 4;
        };
        this.Content = splash;
        await Task.Delay(2500);
#endif
        App.rootFrame.Navigate(typeof(MainPage));
    }
}
