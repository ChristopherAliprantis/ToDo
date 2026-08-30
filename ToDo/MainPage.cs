namespace ToDo;

using System.Text.Json;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;
using Path = System.IO.Path;

public sealed partial class MainPage : Page // #if DESKTOP for all of skia desktop, #if WINDOWS for windows, #if ANDROID for android.
{
    public static Windows.Foundation.Rect bounds;
    public static ToDos todos = new();
    public static List<ToDos.ToDo?>? TODOS = new(0);
    public static double h;
    public static double w;
    public static double avail;
    public static Button? NEW;
    public static Button? SETTINGS;
    public RotateTransform rotationTransform = new Microsoft.UI.Xaml.Media.RotateTransform();
    public static Grid? H;
    public MainPage()
    {
        var Bar = new StackPanel
        {
            Height = 0,
            Width = 0,
        };
        H = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition{ Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }
            }
        };

        var rs = new Microsoft.UI.Xaml.Media.Imaging.SvgImageSource(new Uri("ms-appx:///Assets/reload"));
        var reloadpic = new Microsoft.UI.Xaml.Controls.Image
        {
            Source = rs,
            Height = 40,
            Width = 40,
            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
            RenderTransform = rotationTransform
        };

        var content = new StackPanel
        {
            Spacing = 0,
            Children =
            {
                new Button
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    Content = reloadpic,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0.1),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                },
                todos,
            }

        };
        App.Themes.ThemeChanged += async (s, e) =>
        {
            App.ThemeMode = App.LoadThemeMode(App.Theme);
            if (App.ThemeMode == "Light")
            {
                App.rootFrame.RequestedTheme = ElementTheme.Light;
            }
            else if (App.ThemeMode == "Dark")
            {
                App.rootFrame.RequestedTheme = ElementTheme.Dark;
            }
            if (App.ThemeMode == "Light")
            {
                H.Background = new SolidColorBrush(Colors.White);
                Bar.Background = new SolidColorBrush(Colors.LightGray);
            }
            else if (App.ThemeMode == "Dark")
            {
                H.Background = new SolidColorBrush(Colors.Black);
                Bar.Background = new SolidColorBrush(Colors.FromARGB(255, 58, 58, 58));
            }
            RebuildTodos();
        };
        var hoverBrush = new SolidColorBrush(ColorHelper.FromArgb(132, 235, 235, 235));
        ((Button)content.Children[0]).Resources["ButtonBackgroundPointerOver"] = hoverBrush;
        ((Button)content.Children[0]).Click += async (s, e) =>
        {
            await Reload();
        };
        NEW = new Button
        {
            BorderThickness = new Thickness(0),
            Content = "New",
            Background = new SolidColorBrush(Colors.Transparent),
        };

        SETTINGS = new Button
        {
            BorderThickness = new Thickness(0),
            Content = "Settings",
            Background = new SolidColorBrush(Colors.Transparent),
        };

        NEW.Click += (s, e) =>
        {
            App.rootFrame.Navigate(typeof(New));
        };
        Bar.Children.Add(NEW);
        SETTINGS.Click += (s, e) =>
        {
            App.rootFrame.Navigate(typeof(Settings));
        };
        Bar.Children.Add(SETTINGS);
        var Scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
#if DESKTOP
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
#else
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
#endif
            Content = content
        };
        this.SizeChanged += async (s, e) =>
        {
            w = this.ActualWidth;
            h = this.ActualHeight;

            bounds = App.MainWindow.Bounds;
            if (bounds.Width > bounds.Height == false) todos.currentcol = 1;
            else todos.currentcol = 0;
            if (bounds.Width > bounds.Height)
            {
#if DESKTOP || WINDOWS
                Bar.Width = w / 13.5;
#else
                Bar.Width = w / 8.65;
#endif
            }
            else
            {
                Bar.Width = w / 4.8;
            }

            if (bounds.Width > bounds.Height)
            {
                avail = (w - Bar.Width) / 4 - h / 96 * 2.7 * 1.20;
            }
            else
            {
                avail = (w - Bar.Width) - (h / 96 * 2.7 * 2.20);
            }
#if ANDROID
            if (bounds.Width > bounds.Height)
            {
                ((Button)content.Children[0]).Width = h / 11.6;
            }
            else
            {
                ((Button)content.Children[0]).Width = h / 23.5;
            }
#elif DESKTOP || WINDOWS
    ((Button)content.Children[0]).Width = h / 24.0;
#endif
            ((Button)content.Children[0]).Height = ((Button)content.Children[0]).Width;
            reloadpic.Height = ((Button)content.Children[0]).Height;

            reloadpic.Width = ((Button)content.Children[0]).Width;
            ((Button)content.Children[0]).Margin = new Thickness(h / 96 * 2.7, h / 15.0 / 6, 0, 0);
            todos.col1.Children.Clear();
            todos.col2.Children.Clear();
            todos.col3.Children.Clear();
            todos.col4.Children.Clear();
            todos.Spacing = h / 96 * 2.7;
            todos.col1.Spacing = h / 96 * 2.7;

            if (bounds.Width > bounds.Height)
            {
                todos.col2.Spacing = h / 96 * 2.7;
                todos.col3.Spacing = h / 96 * 2.7;
                todos.col4.Spacing = h / 96 * 2.7;
            }
            Bar.Height = h;
            Bar.Spacing = h / 23.0;

            todos.Margin = new Thickness(h / 96 * 2.7, h / 13.0 / 6, 0, 0);


            NEW.Width = Bar.Width - (Bar.Width / 55 * 2);
            NEW.Margin = new Thickness(Bar.Width / 55, Bar.Height / 18, 0, 0);
            NEW.Height = NEW.Width * 0.463757;
            NEW.FontSize = Bar.Width / 3.2;
            SETTINGS.Width = Bar.Width - (Bar.Width / 55 * 2);
            SETTINGS.Margin = new Thickness(Bar.Width / 55, Bar.Height / 18, 0, 0);
            SETTINGS.Height = SETTINGS.Width * 0.463757;
            SETTINGS.FontSize = Bar.Width / 5.5;
            RebuildTodos();
        };
        this.Loaded += async (s, e) =>
        {
            w = this.ActualWidth;
            h = this.ActualHeight;

            bounds = App.MainWindow.Bounds;
            if (bounds.Width > bounds.Height == false) todos.currentcol = 1;
            else todos.currentcol = 0;
            if (bounds.Width > bounds.Height)
            {
#if DESKTOP || WINDOWS
                Bar.Width = w / 13.5;
#else
                Bar.Width = w / 8.65;
#endif
            }
            else
            {
                Bar.Width = w / 4.8;
            }

            if (bounds.Width > bounds.Height)
            {
                avail = (w - Bar.Width) / 4 - h / 96 * 2.7 * 1.20;
            }
            else
            {
                avail = (w - Bar.Width) - (h / 96 * 2.7 * 2.20);
            }
#if ANDROID
            if (bounds.Width > bounds.Height)
            {
                ((Button)content.Children[0]).Width = h / 11.6;
            }
            else
            {
                ((Button)content.Children[0]).Width = h / 23.5;
            }
#elif DESKTOP || WINDOWS
    ((Button)content.Children[0]).Width = h / 24.0;
#endif
            ((Button)content.Children[0]).Height = ((Button)content.Children[0]).Width;
            reloadpic.Height = ((Button)content.Children[0]).Height;

            reloadpic.Width = ((Button)content.Children[0]).Width;
            ((Button)content.Children[0]).Margin = new Thickness(h / 96 * 2.7, h / 15.0 / 6, 0, 0);
            todos.col1.Children.Clear();
            todos.col2.Children.Clear();
            todos.col3.Children.Clear();
            todos.col4.Children.Clear();
            todos.Spacing = h / 96 * 2.7;
            todos.col1.Spacing = h / 96 * 2.7;

            if (bounds.Width > bounds.Height)
            {
                todos.col2.Spacing = h / 96 * 2.7;
                todos.col3.Spacing = h / 96 * 2.7;
                todos.col4.Spacing = h / 96 * 2.7;
            }
            Bar.Height = h;
            Bar.Spacing = h / 23.0;

            todos.Margin = new Thickness(h / 96 * 2.7, h / 13.0 / 6, 0, 0);


            NEW.Width = Bar.Width - (Bar.Width / 55 * 2);
            NEW.Margin = new Thickness(Bar.Width / 55, Bar.Height / 18, 0, 0);
            NEW.Height = NEW.Width * 0.463757;
            NEW.FontSize = Bar.Width / 3.2;
            SETTINGS.Width = Bar.Width - (Bar.Width / 55 * 2);
            SETTINGS.Margin = new Thickness(Bar.Width / 55, Bar.Height / 18, 0, 0);
            SETTINGS.Height = SETTINGS.Width * 0.463757;
            SETTINGS.FontSize = Bar.Width / 5.5;
            RebuildTodos();
        };
        if (App.ThemeMode == "Light")
        {
            H.Background = new SolidColorBrush(Colors.White);
            Bar.Background = new SolidColorBrush(Colors.LightGray);
        }
        else if (App.ThemeMode == "Dark")
        {
            H.Background = new SolidColorBrush(Colors.Black);
            Bar.Background = new SolidColorBrush(Colors.FromARGB(255, 58, 58, 58));
        }
        Helpers.Add(H, Scroll, 0, 1);
        Helpers.Add(H, Bar, 0, 0);
        this.Content = H;
    }

    public async Task Reload()
    {
        this.rotationTransform.Angle = 0;

        var spinAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Microsoft.UI.Xaml.Duration(TimeSpan.FromSeconds(0.4)),
            EasingFunction = new Microsoft.UI.Xaml.Media.Animation.QuadraticEase
            {
                EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseInOut
            }
        };

        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(spinAnimation, rotationTransform);
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(spinAnimation, "Angle");

        var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
        storyboard.Children.Add(spinAnimation);
        storyboard.Begin();

        await Task.Delay(16);

        var tlist = new List<ToDos.ToDo>(MainPage.TODOS);

        foreach (var t in tlist)
        {
            if (t.Date.HasValue && t.Time.HasValue)
            {
                var dt = t.Date.Value.ToDateTime(t.Time.Value);

                if (dt < DateTime.Now)
                {
                    await t.Delete();
                    MainPage.TODOS.Remove(t);
                }
                else
                {
                    Notifications.CancelNotif(t);
                    Notifications.SendNotif(t);
                }
            }
        }

        await todos.Save();
        await todos.Load();
    }

    public static void RebuildTodos()
    {
        if (TODOS == null || TODOS.Count == 0 || TODOS[0]?.content == null)
        {
            return;
        }
        if (bounds.Width > bounds.Height)
        {
            todos.currentcol = 0;
        }
        else
        {
            todos.currentcol = 1;
        }

        for (int i = 0; i < MainPage.todos.Children.Count; i++)
        {
            if (MainPage.todos.Children[i] is StackPanel sp)
            {
                sp.Children.Clear();
            }
        }
        if (TODOS == null)
        {
            TODOS[0] = null;
        }
        for (int i = 0; i <= TODOS.Count - 1; i++)
        {
            if (TODOS[i] == null)
            {
                continue;
            }
            if (TODOS[i].Color == null)
            {
                if (App.ThemeMode == "Light")
                {
                    TODOS[i].border.Background = new SolidColorBrush(Colors.White);
                    TODOS[i].border.BorderBrush = new SolidColorBrush(Colors.Black);
                }
                else if (App.ThemeMode == "Dark")
                {
                    TODOS[i].border.Background = new SolidColorBrush(Colors.Black);
                    TODOS[i].border.BorderBrush = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                TODOS[i].border.Background = new SolidColorBrush(TODOS[i].Color.Value);
                if (App.ThemeMode == "Light")
                {
                    TODOS[i].border.BorderBrush = new SolidColorBrush(Colors.Black);
                }
                else if (App.ThemeMode == "Dark")
                {
                    TODOS[i].border.BorderBrush = new SolidColorBrush(Colors.White);
                }
            }
            TODOS[i].Width = avail;
            TODOS[i].content.Padding = new Thickness(avail / 17, avail / 17, avail / 17, avail / 17);
#if ANDROID
            if (bounds.Width > bounds.Height)
            {
                ((TextBlock)TODOS[i].content.Children[0]).FontSize = NEW.FontSize - 3.7;
            }
            else
            {
                ((TextBlock)TODOS[i].content.Children[0]).FontSize = NEW.FontSize - 0.68;
            }
#else
            ((TextBlock)TODOS[i].content.Children[0]).FontSize = NEW.FontSize - 0.68;
#endif
            ((TextBlock)TODOS[i].content.Children[1]).FontSize = NEW.FontSize - 6.28;

#if DESKTOP || WINDOWS
            ((ComboBox)TODOS[i].content.Children[3]).Width = avail * 0.32;
#else
            ((ComboBox)TODOS[i].content.Children[3]).Width = avail * 0.48;
#endif

            ((TextBlock)TODOS[i].content.Children[2]).FontSize = ((TextBlock)TODOS[i].content.Children[1]).FontSize;

            ((ComboBox)TODOS[i].content.Children[3]).Height = ((ComboBox)TODOS[i].content.Children[3]).Width * 0.387;
            ((ComboBox)TODOS[i].content.Children[3]).FontSize = ((ComboBox)TODOS[i].content.Children[3]).Height / 2.1;
            ((ComboBox)TODOS[i].content.Children[3]).Padding = new Thickness(((ComboBox)TODOS[i].content.Children[3]).Width / 5, 0, 0, 0);
            todos.AddBack(TODOS[i]);
        }
    }
}

public class Helpers
{
    public static void Add(Grid grid, FrameworkElement which, int row, int col)
    {
        grid.Children.Remove(which);
        grid.Children.Add(which);
        Grid.SetRow(which, row);
        Grid.SetColumn(which, col);
    }
}

public interface INotificationService
{
    void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData);
    void CancelNotification(string actionData);

    void ShowImmediate(string title, string message);
}



public partial class ToDos : StackPanel
{
    public StackPanel col1 = new StackPanel();
    public StackPanel col2 = new StackPanel();
    public StackPanel col3 = new StackPanel();
    public StackPanel col4 = new StackPanel();
    public int currentcol = 1;
    public ToDos()
    {
        Orientation = Orientation.Horizontal;
        Children.Add(col1);
        Children.Add(col2);
        Children.Add(col3);
        Children.Add(col4);

    }
    public partial class ToDo : UserControl
    {
        public Border border;
        public StackPanel content;
        public DateOnly? Date { get; set; }
        public TimeOnly? Time { get; set; }
        public string Title;
        public string Descrip;

        public string? DTime;
        public string? DDate;
        public string? ID;
        public Windows.UI.Color? Color;
        public ToDo(string title, string descrip, DateOnly? date, TimeOnly? time, string? id, Windows.UI.Color? color)
        {
            Title = title;
            Descrip = descrip;
            Date = date;
            Time = time;
            ID = id;
            Color = color;
            if (Date == null)
            {
                DDate = "";
            }
            else
            {
                DDate = ((DateOnly)Date).ToString("yyyy-MM-dd");
            }
            if (Time == null)
            {
                DTime = "";
            }
            else
            {
                DTime = ((TimeOnly)Time).ToString("hh:mm tt");
            }
            content = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = Title,
                        TextWrapping = TextWrapping.Wrap,
                        IsTextSelectionEnabled = true,
                    },
                    new TextBlock
                    {
                        Text = Descrip,
                        TextWrapping = TextWrapping.Wrap,
                        IsTextSelectionEnabled = true,
                    },
                    new TextBlock
                    {
                        IsTextSelectionEnabled = true,
                        TextWrapping = TextWrapping.Wrap,
                        Text = $"{DDate}\n{DTime}"
                    },
                    new ComboBox
                    {
                        PlaceholderText = ". . .",

                        Items =
                        {
                            new ComboBoxItem
                            {
                                Content = "Delete",
                            },
                            new ComboBoxItem
                            {
                                Content = "Edit",
                            }
                        }
                    },
                }
            };
            Content = border = new Border
            {
                BorderThickness = new Thickness(1.3),
                BorderBrush = new SolidColorBrush(Colors.Black),
                Width = this.Width,
                Height = this.Height,
                CornerRadius = new CornerRadius(5.2),
                Child = content,
            };
            ((ComboBox)content.Children[3]).DropDownOpened += async (s, e) =>
            {
                var combo = (ComboBox)content.Children[3];

                ((ComboBoxItem)((ComboBox)content.Children[3]).Items[0]).FontSize = combo.Width / 5;
                ((ComboBoxItem)((ComboBox)content.Children[3]).Items[1]).FontSize = combo.Width / 5;
            };
            ((ComboBox)content.Children[3]).SelectionChanged += async (s, e) =>
            {
                var combo = (s as ComboBox);
                if (combo == null || combo.SelectedIndex == -1) return;

                var item = combo.SelectedItem as ComboBoxItem;
                string choice = item?.Content?.ToString();

                combo.SelectedIndex = -1;

                if (choice == "Edit")
                {
                    New.edit = (this, true);
                    App.rootFrame.Navigate(typeof(New));
                }
                else if (choice == "Delete")
                {
                    await Delete();
                }
            };
            if (Color == null)
            {
                if (App.ThemeMode == "Light")
                {
                    border.Background = new SolidColorBrush(Colors.White);
                    border.BorderBrush = new SolidColorBrush(Colors.Black);
                }
                else if (App.ThemeMode == "Dark")
                {
                    border.Background = new SolidColorBrush(Colors.Black);
                    border.BorderBrush = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                border.Background = new SolidColorBrush(Color.Value);
                if (App.ThemeMode == "Light")
                {
                    border.BorderBrush = new SolidColorBrush(Colors.Black);
                }
                else if (App.ThemeMode == "Dark")
                {
                    border.BorderBrush = new SolidColorBrush(Colors.White);
                }
            }
        }



        public async Task Delete()
        {
            Console.WriteLine($"Deleting ToDo: '{ID}'");
            if (ID != null) Console.WriteLine("ID is not null");
            for (int i = 0; i < MainPage.todos.Children.Count; i++)
            {
                ((StackPanel)MainPage.todos.Children[i]).Children.Clear();
            }

            if (!string.IsNullOrWhiteSpace(ID))
            {
                Console.WriteLine($"Cancelling notification with ID: {ID}");
                Notifications.CancelNotif(this);
            }
            MainPage.TODOS.Remove(this);
            await MainPage.todos.Save();
            await MainPage.todos.Load();
        }

        public static async Task DeleteById(string ID)
        {
            int i = 0;
            int pos = 0;
            foreach (ToDo t in (MainPage.TODOS))
            {
                if (t.ID == ID)
                {
                    pos = i;
                }
                i++;
            }
            await MainPage.TODOS[pos].Delete();
        }
    }
    public async Task ADD(string title, string descrip, DateOnly? date, TimeOnly? time, string? id, Color? Color)
    {
        var N = new ToDo(title, descrip, date, time, id, Color);
        if (N.ID != null)
        {
            Notifications.SendNotif(N);
        }
        MainPage.TODOS.Add(N);
        await MainPage.todos.Save();
    }

    public async Task ADD(string title, string descrip, DateOnly? date, TimeOnly? time, string? id, int index, Color? Color)
    {
        var N = new ToDo(title, descrip, date, time, id, Color);
        if (N.ID != null)
        {
            Notifications.SendNotif(N);
        }
        MainPage.TODOS[index] = N;
        await MainPage.todos.Save();
    }
    public void AddBack(ToDo? thing)
    {
        if (thing == null)
        {
            return;
        }
        if (currentcol == 4)
        {
            currentcol = 0;
        }
        if (MainPage.bounds.Width > MainPage.bounds.Height)
        {
            currentcol++;
        }
        if (currentcol == 1)
        {
            MainPage.todos.col1.Children.Add(thing);
        }
        else if (currentcol == 2)
        {
            MainPage.todos.col2.Children.Add(thing);
        }
        else if (currentcol == 3)
        {
            MainPage.todos.col3.Children.Add(thing);
        }
        else if (currentcol == 4)
        {
            MainPage.todos.col4.Children.Add(thing);
        }
    }

    public async Task Save()
    {
        try
        {
            List<ToDoData> todos = new();

            foreach (var t in MainPage.TODOS)
            {
                if (t == null) continue;

                todos.Add(new ToDoData
                {
                    Title = t.Title,
                    Descrip = t.Descrip,
                    Date = t.Date,
                    Time = t.Time,
                    ID = t.ID,
                    Color = CH.ColorToHex(t.Color),
                });
            }

            string jsonData = JsonSerializer.Serialize(todos);

            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToDo");

            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "todos.json");

            await File.WriteAllTextAsync(filePath, jsonData);
            Console.WriteLine("ToDos saved");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public async Task Load()
    {
        try
        {
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToDo");

            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "todos.json");

            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, "[]");
            }

            string jsonData = await File.ReadAllTextAsync(filePath);

            var todos = JsonSerializer.Deserialize<List<ToDoData>>(jsonData);

            if (todos == null)
                return;

            MainPage.TODOS.Clear();

            foreach (var d in todos)
            {
                MainPage.TODOS.Add(
                    new ToDos.ToDo(
                        d.Title ?? "",
                        d.Descrip ?? "",
                        d.Date,
                        d.Time,
                        d.ID,
                        CH.HexToColor(d.Color)
                    )
                );
            }

            MainPage.RebuildTodos();
            Console.WriteLine("ToDos loaded");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

}

public class ToDoData
{
    public string? Title { get; set; }

    public string? Descrip { get; set; }

    public DateOnly? Date { get; set; }

    public TimeOnly? Time { get; set; }

    public string? ID { get; set; }

    public string? Color { get; set; } = null;
}
public static class CH
{
    public static string? ColorToHex(Color? color)
    {
        if (color == null) return null;
        return $"#{color.Value.A:X2}{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}";
    }

    public static Color? HexToColor(string? hex)
    {
        if (hex == null) return null;
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new ArgumentException("Hex string cannot be null or empty.", nameof(hex));
        }

        hex = hex.Trim().TrimStart('#');

        if (hex.Length == 6)
        {
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromArgb(255, r, g, b);
        }
        else if (hex.Length == 8)
        {
            byte a = Convert.ToByte(hex.Substring(0, 2), 16);
            byte r = Convert.ToByte(hex.Substring(2, 2), 16);
            byte g = Convert.ToByte(hex.Substring(4, 2), 16);
            byte b = Convert.ToByte(hex.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        throw new FormatException($"Invalid hex color string length: {hex.Length}. Input: '{hex}'");
    }
}
