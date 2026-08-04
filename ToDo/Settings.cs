using Microsoft.UI.Xaml.Media.Imaging;
using ToDo;
using Path = System.IO.Path;
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
        int index = 0;
        var As = new SvgImageSource(new Uri("ms-appx:///Assets/arrow"));
        if (App.Theme == "Dark")
        {
            index = 0;
        }
        else if (App.Theme == "Light")
        {
            index = 1;
        }
        else if (App.Theme == "System")
        {
            index = 2;
        }
        StackPanel theme = new StackPanel
        { 
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock
                {
                    Text = "Theme: ",
                    FontSize = 20,
                },
                new ComboBox
                {
                    Width = 150,
                    Items =
                    {
                        new ComboBoxItem
                        {
                            Content = "Dark",
                        },
                        new ComboBoxItem
                        {
                            Content = "Light",
                        },
                        new ComboBoxItem
                        {
                            Content = "System",
                        }
                    },
                    SelectedIndex = index
                }
            }
        };

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
        var THEME = (ComboBox)theme.Children[1];
        THEME.SelectionChanged += async (s, e) =>
        {
            await UpdateTheme(((ComboBoxItem)THEME.Items[THEME.SelectedIndex]).Content.ToString());
        };
        void ResizeButton()
        {
            double size = ActualHeight / 27.0;

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

        back.Click += async(s, e) =>
        {
            App.rootFrame.Navigate(typeof(MainPage));
            await UpdateTheme(((ComboBoxItem)THEME.Items[THEME.SelectedIndex]).Content.ToString());
        };

        Helpers.Add(S, back, 0, 0);

        Content = S;
    }

    public async Task UpdateTheme(string theme)
    {
        App.Theme = theme;
        string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToDo");

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "theme.txt");
        await File.WriteAllTextAsync(filePath, theme);
    }
}
