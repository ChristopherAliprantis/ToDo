namespace ToDo;

using System.Text.Json;




public sealed partial class MainPage : Page // #if __DESKTOP__ for all of skia desktop, #if WINDOWS for windows, #if __ANDROID__ for android.
{
    public static Windows.Foundation.Rect bounds;
    public static ToDos todos = new();
    public static List<ToDos.ToDo?>? TODOS = new();
    public static double h;
    public static double w;
    public static double avail;
    public static Button? NEW;
    public static Grid? H;
    public MainPage()
    {
        todos.Load();
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
#if __DESKTOP__
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
#else
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
#endif
            Content = todos
        };
        this.SizeChanged += (s, e) =>
        {
            w = this.ActualWidth;
            h = this.ActualHeight;

            bounds = App.MainWindow.Bounds;
            if (bounds.Width > bounds.Height == false) todos.currentcol = 1;
            else todos.currentcol = 0;
            if (bounds.Width > bounds.Height)
            {
#if __DESKTOP__
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
                avail = H.ColumnDefinitions[1].ActualWidth / 4 - h / 96 * 2.7 * 1.20;
            }
            else
            {
                avail = H.ColumnDefinitions[1].ActualWidth - (h / 96 * 2.7 * 2.20);
            }
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
            

            Scroll.Margin = new Thickness(h / 96 * 2.7, h / 9.2, 0, 0);


            NEW.Width = Bar.Width - (Bar.Width / 55 * 2);
            NEW.Margin = new Thickness(Bar.Width / 55, Bar.Height / 18, 0, 0);
            NEW.Height = NEW.Width * 0.463757;
            NEW.FontSize = Bar.Width / 3.2;
            RebuildTodos();
        };
        this.Loaded += (s, e) =>
        {
            w = this.ActualWidth;
            h = this.ActualHeight;

            bounds = App.MainWindow.Bounds;
            if (bounds.Width > bounds.Height == false) todos.currentcol = 1;
            else todos.currentcol = 0;
            if (bounds.Width > bounds.Height)
            {
#if __DESKTOP__
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
                avail = H.ColumnDefinitions[1].ActualWidth / 4 - h / 96 * 2.7 * 1.20;
            }
            else
            {
                avail = H.ColumnDefinitions[1].ActualWidth - (h / 96 * 2.7 * 2.20);
            }
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
            NEW.Width = Bar.ActualWidth - (Bar.ActualWidth / 55 * 2);
            NEW.Margin = new Thickness(Bar.ActualWidth / 55, Bar.ActualHeight / 18, 0, 0);
            NEW.Height = NEW.Width * 0.463757;
            NEW.FontSize = Bar.ActualWidth / 3.2;

            Scroll.Margin = new Thickness(h / 96 * 2.7, h / 9.2, 0, 0);
            
            RebuildTodos();
            
        };
        Helpers.Add(H, Scroll, 1, 1);
        Helpers.Add(H, Bar, 0, 0);
        this.Content = H;
    }
    public static void RebuildTodos()
    {
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
#if __ANDROID__
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
#if __DESKTOP__
            ((Button)TODOS[i].content.Children[2]).Width = avail * 0.32;
#else
            ((Button)TODOS[i].content.Children[2]).Width = avail * 0.48;
#endif
            ((Button)TODOS[i].content.Children[2]).Height = ((Button)TODOS[i].content.Children[2]).Width * 0.46;
            ((Button)TODOS[i].content.Children[2]).FontSize = ((Button)TODOS[i].content.Children[2]).Width / 4.86;
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
        public string Title;
        public string Descrip;
        public ToDo(string title, string descrip)
        {
            Title = title;
            Descrip = descrip;
            content = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = Title,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = Descrip,
                        TextWrapping = TextWrapping.Wrap
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
            ((Button)content.Children[2]).Click += async (s, e) =>
            {
                for (int i = 0; i < MainPage.todos.Children.Count; i++)
                {
                    ((StackPanel)MainPage.todos.Children[i]).Children.Clear();
                }
                MainPage.TODOS.Remove(this);
                MainPage.todos.Save();
                MainPage.todos.Load();
            };
        }
    }
    public void ADD(string title, string descrip)
    {
        var N = new ToDo(title, descrip);
        MainPage.TODOS.Add(N);
        MainPage.todos.Save();
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

    public async void Save()
    {
        List<ToDoData> todos = new();

        for (int i = 0; i < MainPage.TODOS.Count; i++)
        {
            var t = MainPage.TODOS[i];
            if (t == null) continue;

            todos.Add(new ToDoData
            {
                Title = t.Title,
                Descrip = t.Descrip
            });
        }

        string jsonData = JsonSerializer.Serialize(todos);

        StorageFolder local = ApplicationData.Current.LocalFolder;

        StorageFolder folder =
            await local.CreateFolderAsync("ToDo", CreationCollisionOption.OpenIfExists);

        StorageFile file =
            await folder.CreateFileAsync("todos.json", CreationCollisionOption.ReplaceExisting);

        await FileIO.WriteTextAsync(file, jsonData);
    }


    public async void Load()
    {
        StorageFolder local = ApplicationData.Current.LocalFolder;

        StorageFolder folder =
            await local.CreateFolderAsync("ToDo", CreationCollisionOption.OpenIfExists);

        StorageFile file;

        try
        {
            file = await folder.GetFileAsync("todos.json");
        }
        catch
        {
            file = await folder.CreateFileAsync(
                "todos.json",
                CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, "[]");
            return;
        }

        string jsonData = await FileIO.ReadTextAsync(file);

        var todos =
            JsonSerializer.Deserialize<List<ToDoData>>(jsonData);

        if (todos == null) return;

        MainPage.TODOS.Clear();

        foreach (var d in todos)
        {
            MainPage.TODOS.Add(
                new ToDos.ToDo(
                    d.Title ?? "",
                    d.Descrip ?? ""));
        }

        MainPage.RebuildTodos();
    }

}

public class ToDoData
{
    public string? Title { get; set; }
    public string? Descrip { get; set; }
}
