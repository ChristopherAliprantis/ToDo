using Microsoft.UI.Xaml.Media.Imaging;
using ToDo;

public sealed partial class Settings : Page
{
    public static Grid S = new Grid();
    public Settings()
    {
        S = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            RowDefinitions =
        {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
        },
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        }
        };

        var As = new SvgImageSource(new Uri("ms-appx:///Assets/arrow.svg"));

        var arrowpic = new Image
        {
            Source = As,
            Stretch = Stretch.Uniform
        };

        var back = new Button
        {
            Content = arrowpic,
            Background = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        var hoverBrush = new SolidColorBrush(ColorHelper.FromArgb(132, 235, 235, 235));
        back.Resources["ButtonBackgroundPointerOver"] = hoverBrush;

        void ResizeButton()
        {
            double size = ActualHeight / 30.0;

            back.Width = size;
            back.Height = size;

            arrowpic.Width = size;
            arrowpic.Height = size;
        }

        SizeChanged += (s, e) =>
        {
            ResizeButton();
        };

        Loaded += (s, e) =>
        {
            ResizeButton();
        };

        back.Click += (s, e) =>
        {
            App.rootFrame.Navigate(typeof(MainPage));
        };

        Helpers.Add(S, back, 0, 0);

        Content = S;
    }
}
