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
            VerticalAlignment = VerticalAlignment.Top,
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
            if (App.ThemeMode == "Light")
            {
                S.Background = new SolidColorBrush(Colors.White);
            }
            else if (App.ThemeMode == "Dark")
            {
                S.Background = new SolidColorBrush(Colors.Black);
            }
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
            theme.Spacing = ActualWidth / 15.0;
            ((TextBlock)theme.Children[0]).Height = ActualHeight / 15.0;
            ((TextBlock)theme.Children[0]).FontSize = ((TextBlock)theme.Children[0]).Height / 2.0;
            theme.Width = ActualWidth;
            theme.Height = ActualHeight / 15.0;
            THEME.Height = theme.Height;
            THEME.Width = THEME.Height * 4;
            THEME.FontSize = THEME.Height / 2.0;
            foreach (var item in THEME.Items)
            {
                if (item is ComboBoxItem comboBoxItem)
                {
                    comboBoxItem.FontSize = THEME.FontSize;
                }
            }
            theme.Margin = new Thickness(ActualWidth / 20.0, ActualHeight / 20.0, 0, 0);
        };

        Loaded += (s, e) =>
        {
            ResizeButton();
            theme.Spacing = ActualWidth / 15.0;
            ((TextBlock)theme.Children[0]).Height = ActualHeight / 15.0;
            ((TextBlock)theme.Children[0]).FontSize = ((TextBlock)theme.Children[0]).Height / 2.0;
            theme.Width = ActualWidth;
            theme.Height = ActualHeight / 15.0;
            THEME.Height = theme.Height;
            THEME.Width = THEME.Height * 4;
            THEME.FontSize = THEME.Height / 2.0;
            foreach (var item in THEME.Items)
            {
                if (item is ComboBoxItem comboBoxItem)
                {
                    comboBoxItem.FontSize = THEME.FontSize;
                }
            }
            theme.Margin = new Thickness(ActualWidth / 20.0, ActualHeight / 20.0, 0, 0);
        };

        back.Click += async(s, e) =>
        {
            await UpdateTheme(((ComboBoxItem)THEME.Items[THEME.SelectedIndex]).Content.ToString());
            if (App.ThemeMode == "Light")
            {
                S.Background = new SolidColorBrush(Colors.White);
            }
            else if (App.ThemeMode == "Dark")
            {
                S.Background = new SolidColorBrush(Colors.Black);
            }
            App.rootFrame.Navigate(typeof(MainPage));
        };
        if (App.ThemeMode == "Light")
        {
            S.Background = new SolidColorBrush(Colors.White);
        }
        else if (App.ThemeMode == "Dark")
        {
            S.Background = new SolidColorBrush(Colors.Black);
        }
        Helpers.Add(S, back, 0, 0);
        Helpers.Add(S, theme, 1, 1);

        Content = S;
    }

    public static async Task UpdateTheme(string theme)
    {
        App.Theme = theme;
        string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToDo");

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "theme.txt");
        await File.WriteAllTextAsync(filePath, theme);
        App.ThemeMode = App.LoadThemeMode(theme);
        if (App.ThemeMode == "Light")
        {
            App.rootFrame.RequestedTheme = ElementTheme.Light;
        }
        else if (App.ThemeMode == "Dark")
        {
            App.rootFrame.RequestedTheme = ElementTheme.Dark;
        }
    }
}
