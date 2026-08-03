using ToDo;

public sealed partial class Settings : Page
{
    public static Grid S = new Grid();
    public Settings()
    {
        S = new Grid
        {
            Width = this.Width, Height = this.Height,
            RowDefinitions =
            {
                new RowDefinition{ Height = new GridLength(0, GridUnitType.Auto) },
                new RowDefinition{ Height = new GridLength(1, GridUnitType.Star) }

            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }
            }
        };
        var As = new Microsoft.UI.Xaml.Media.Imaging.SvgImageSource(new Uri("ms-appx:///Assets/arrow"));
        var arrowpic = new Microsoft.UI.Xaml.Controls.Image
        {
            Source = As,
            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
        };
        var back = new Button
        {
            Content = arrowpic,
            Background = new SolidColorBrush(Color.Transparent),
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0.1),
            BorderBrush = new SolidColorBrush(Color.Transparent),
            Width = 40,
            Height = 40,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };
        var hoverBrush = new SolidColorBrush(ColorHelper.FromArgb(132, 235, 235, 235));
        back.Resources["ButtonBackgroundPointerOver"] = hoverBrush;
        this.Loaded += async(s, e) =>
        {
            S.Width = this.Width;
            S.Height = this.Height;
            back.Height = (S.Height / 30.0);
            back.Width = back.Height;
        };
        this.SizeChanged += async(s, e) =>
        {
            S.Width = this.Width;
            S.Height = this.Height;
            back.Height = (S.Height / 30.0);
            back.Width = back.Height;
        };
        back.Click += async(s, e) =>
        {
            App.rootFrame.Navigate(typeof(MainPage));
        };
        Helpers.Add(S, back, 0,0);
        this.Content = S;
    }
}
