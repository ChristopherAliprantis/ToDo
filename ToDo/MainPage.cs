namespace ToDo;
using System.Text.Json;
using Microsoft.UI.Xaml.Media.Imaging;
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
    public RotateTransform rotationTransform = new Microsoft.UI.Xaml.Media.RotateTransform();
    public static Grid? H;
    public MainPage()
    {
        todos.Load();
        todos.Save();
        var Bar = new StackPanel
        {
            Height = 0,
            Width = 0,
            Background = new SolidColorBrush(Color.LightGray),
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
        var rs = new Microsoft.UI.Xaml.Media.Imaging.SvgImageSource(new Uri("ms-appx:///Assets/reload.svg"));
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
                    Background = new SolidColorBrush(Color.Transparent),
                    Content = reloadpic,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                },
                todos,
            }

        };
        ((Button)content.Children[0]).Click += (s, e) =>
        {
            Reload();
        };
        NEW = new Button
        {
            BorderThickness = new Thickness(0),
            Content = "New",
            Background = new SolidColorBrush(Color.Transparent),
        };

        NEW.Click += (s, e) =>
        {
            App.rootFrame.Navigate(typeof(New));
        };
        Bar.Children.Add(NEW);
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
        this.SizeChanged += async(s, e) =>
        {
            w = this.ActualWidth;
            h = this.ActualHeight;

            bounds = App.MainWindow.Bounds;
            if (bounds.Width > bounds.Height == false) todos.currentcol = 1;
            else todos.currentcol = 0;
            if (bounds.Width > bounds.Height)
            {
#if DESKTOP || WINDOWS
                Bar.Width = w / 14.7;
#else
                Bar.Width = w / 8.6;
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
            ((Button)content.Children[0]).Width = h / 18.0;
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


            todos.Margin = new Thickness(h / 96 * 2.7, h / 13.0 / 6, 0, 0);


            NEW.Width = Bar.Width - (Bar.Width / 55 * 2);
            NEW.Margin = new Thickness(Bar.Width / 55, Bar.Height / 18, 0, 0);
            NEW.Height = NEW.Width * 0.463757;
            NEW.FontSize = Bar.Width / 3.2;
            RebuildTodos();
        };
        this.Loaded += async(s, e) =>
        {
            w = this.ActualWidth;
            h = this.ActualHeight;

            bounds = App.MainWindow.Bounds;
            if (bounds.Width > bounds.Height == false) todos.currentcol = 1;
            else todos.currentcol = 0;
            if (bounds.Width > bounds.Height)
            {
#if DESKTOP || WINDOWS
                Bar.Width = w / 14.7;
#else
                Bar.Width = w / 8.6;
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
            ((Button)content.Children[0]).Width = h / 18.0;
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


            todos.Margin = new Thickness(h / 96 * 2.7, h / 13.0 / 6, 0, 0);


            NEW.Width = Bar.Width - (Bar.Width / 55 * 2);
            NEW.Margin = new Thickness(Bar.Width / 55, Bar.Height / 18, 0, 0);
            NEW.Height = NEW.Width * 0.463757;
            NEW.FontSize = Bar.Width / 3.2;
            await Task.Delay(150);
            Reload();
        };
        Helpers.Add(H, Scroll, 0, 1);
        Helpers.Add(H, Bar, 0, 0);
        this.Content = H;
    }

    public void Reload()
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
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(spinAnimation, new Microsoft.UI.Xaml.PropertyPath("(RotateTransform.Angle)"));

        var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
        storyboard.Children.Add(spinAnimation);
        storyboard.Begin();
        var tlist = new List<ToDos.ToDo>(MainPage.TODOS);
        foreach (var ToDo in MainPage.TODOS)
        {
            
        }

        todos.Save();
        RebuildTodos();
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
            ((Button)TODOS[i].content.Children[3]).Width = avail * 0.32;
#else
            ((Button)TODOS[i].content.Children[3]).Width = avail * 0.48;
#endif

            ((TextBlock)TODOS[i].content.Children[2]).FontSize = ((TextBlock)TODOS[i].content.Children[1]).FontSize;

            ((Button)TODOS[i].content.Children[3]).Height = ((Button)TODOS[i].content.Children[3]).Width * 0.46;
            ((Button)TODOS[i].content.Children[3]).FontSize = ((Button)TODOS[i].content.Children[3]).Width / 4.86;
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
        public ToDo(string title, string descrip, DateOnly? date, TimeOnly? time, string? id)
        {
            Title = title;
            Descrip = descrip;
            Date = date;
            Time = time;
            ID = id;
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
                    new Button
                    {
                        Content = "Delete"
                    }
                }
            };
            Content = border = new Border
            {
                BorderThickness = new Thickness(1.3),
                BorderBrush = new SolidColorBrush(Color.Black),
                Width = this.Width,
                Height = this.Height,
                CornerRadius = new CornerRadius(5.2),
                Background = new SolidColorBrush(Color.White),
                Child = content,
            };
            ((Button)content.Children[3]).Click += async (s, e) =>
            {
                await Delete();
            };
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
                await Task.Run(() => App.NotificationService.CancelNotification(ID));
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
    public async Task ADD(string title, string descrip, DateOnly? date, TimeOnly? time, string? id)
    {
        var N = new ToDo(title, descrip, date, time, id);
        if (N.ID != null)
        {
            await Notifications.SendNotif(N);
        }
        MainPage.TODOS.Add(N);
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
                    ID = t.ID
                });
            }

            string jsonData = JsonSerializer.Serialize(todos);

            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToDo");

            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "todos.json");

            await File.WriteAllTextAsync(filePath, jsonData);
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
                        d.ID));
            }

            MainPage.RebuildTodos();
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
}
