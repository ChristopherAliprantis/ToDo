using Uno.Diagnostics.Eventing;
using static ToDo.ToDos;
namespace ToDo;


public sealed partial class New : Page // #if DESKTOP for all of skia desktop, #if WINDOWS for windows, #if ANDROID for android.
{
    public static (ToDos.ToDo?, bool) edit = (null, false);
    public New()
    {
        RowDefinition? space;
        var N = new Grid
        {
            RowDefinitions =
            {
                (space = new RowDefinition { Height = new GridLength(0, GridUnitType.Pixel)}),
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star)}
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)}
            }
        };
        StackPanel? all = new();
        Button? back;
        Rectangle? divide;
        TextBox? title;
        Button? done;
        TimeOnly Time = TimeOnly.FromDateTime(DateTime.Now);
        DateOnly Date = DateOnly.FromDateTime(DateTime.Now);
        TextBox? describe;
        UpDownBox? date;
        UpDownBox? time;
        StackPanel? times = new();
        Switch? Ti;
        Switch? Co;
        CPWbutton? color;
        all = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                (back = new Button
                {
                    Content = "Cancel",
                    HorizontalAlignment = HorizontalAlignment.Left
                }),
                (done = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Content = "Done"
                }),
                (title = new TextBox
                {
                    PlaceholderText = "Title",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    AcceptsReturn = true
                }),
                (describe = new TextBox
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    PlaceholderText = "Description",
                    AcceptsReturn = true
                }),
                (divide = new Rectangle
                {
                    Fill = new SolidColorBrush(Color.Black)
                }),
                (Co = new Switch
                {
                    IsOn = false,
                    MinWidth = 0,
                    MinHeight = 0,
                    HorizontalAlignment = HorizontalAlignment.Left
                }),

                (color = new CPWbutton
                {
                    IsEnabled = false,
                    HorizontalAlignment = HorizontalAlignment.Left
                    
                }),
                (Ti = new Switch
                {
                    IsOn = false,
                    MinWidth = 0,
                    MinHeight = 0,
                    HorizontalAlignment = HorizontalAlignment.Left
                }),

                 (date = new UpDownBox
                 {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = Date.ToString("d"),
                    disabled = !(bool)Ti.IsOn
                 }),
                 (time = new UpDownBox
                 {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = Time.ToString("hh:mm tt"),
                    disabled = !(bool)Ti.IsOn
                 }),

            }

        };
        if (edit.Item2 == true && edit.Item1 != null)
        {
            if (edit.Item1.Date != null) Date = edit.Item1.Date.Value;
            if (edit.Item1.Time != null) Time = edit.Item1.Time.Value;
            title.Text = edit.Item1.Title;
            describe.Text = edit.Item1.Descrip;
            if (edit.Item1.Date != null)
            {
                Ti.IsOn = true;
                date.Text = Date.ToString("yyyy-MM-dd");
                time.Text = Time.ToString("hh:mm tt");
            }
            if (edit.Item1.Color != null)
            {
                color.IsEnabled = true;
                color.cpw.cp.Color = edit.Item1.Color.Value;
            }
            else
            {
                if (App.ThemeMode == "Light")
                {
                    color.cpw.cp.Color = Color.White;
                }
                else if (App.ThemeMode == "Dark")
                {
                    color.cpw.cp.Color = Color.Black;
                }
            }

        }
        Co.Toggled += (s, e) =>
        {
            color.IsEnabled = (bool)Co.IsOn;
            if (Co.IsOn == false)
            {
                if (App.ThemeMode == "Light")
                {
                    color.cpw.cp.Color = Color.White;
                }
                else if (App.ThemeMode == "Dark")
                {
                    color.cpw.cp.Color = Color.Black;
                }
                
            }
        };
        time.up.Click += (s, e) =>
        {
            if (Time != TimeOnly.MaxValue)
            {
                Time = Time.AddMinutes(1);
                time.Text = Time.ToString("hh:mm tt");
                time.text.Text = time.Text;
            }
            else
            {
                Time = TimeOnly.MinValue;
                time.Text = Time.ToString("hh:mm tt");
                time.text.Text = time.Text;
            }
        };
        time.down.Click += (s, e) =>
        {
            if (Time != TimeOnly.MinValue)
            {
                Time = Time.AddMinutes(-1);
                time.Text = Time.ToString("hh:mm tt");
                time.text.Text = time.Text;
            }
            else
            {
                Time = TimeOnly.MaxValue;
                time.Text = Time.ToString("hh:mm tt");
                time.text.Text = time.Text;
            }
        };
        date.up.Click += (s, e) =>
        {
            if (Date != DateOnly.MaxValue)
            {
                Date = Date.AddDays(1);
                date.Text = Date.ToString("d");
                date.text.Text = date.Text;
            }
            else
            {
                Date = DateOnly.MinValue;
                date.Text = Date.ToString("d");
                date.text.Text = date.Text;
            }
        };
        date.down.Click += (s, e) =>
        {
            if (Date != DateOnly.MinValue)
            {
                Date = Date.AddDays(-1);
                date.Text = Date.ToString("d");
                date.text.Text = date.Text;
            }
            else
            {
                Date = DateOnly.MaxValue;
                date.Text = Date.ToString("d");
                date.text.Text = date.Text;
            }
        };
        back.Click += (s, e) =>
        {
            App.rootFrame.Navigate(typeof(MainPage));
        };
        done.Click += async (s, e) =>
        {
            int? ind = null;
            if (Ti.IsOn == false)
            {
                if (edit.Item2 == true && edit.Item1 != null)
                {
                    var t = edit.Item1;
                    ind = MainPage.TODOS.IndexOf(edit.Item1);
                    if (ind == -1)
                    {
                        ind = null;
                    }
                }
                if (ind == null) await MainPage.todos.ADD(title.Text, describe.Text, null, null, null, color.cpw.cp.Color);
                else await MainPage.todos.ADD(title.Text, describe.Text, null, null, null, ind.Value, color.cpw.cp.Color);
                if (edit.Item2 == true && edit.Item1 != null) await edit.Item1.Delete();
                edit = (null, false);
                await MainPage.todos.Save();
                await MainPage.todos.Load();
            }
            else
            {
                if (Date.ToDateTime(Time) < DateTime.Now) return;
                if (edit.Item2 == true && edit.Item1 != null)
                {

                    var t = edit.Item1;
                    ind = MainPage.TODOS.IndexOf(edit.Item1);
                    if (ind == -1)
                    {
                        ind = null;
                    }
                }
                string ID = System.Guid.NewGuid().ToString();
                Console.WriteLine($"New ToDo ID: {ID}");
                if (ind == null) await MainPage.todos.ADD(title.Text, describe.Text, Date, Time, ID, color.cpw.cp.Color);
                else await MainPage.todos.ADD(title.Text, describe.Text, Date, Time, ID, ind.Value, color.cpw.cp.Color);
                if (edit.Item2 == true && edit.Item1 != null)
                {
                    await edit.Item1.Delete();
                }
                edit = (null, false);
                await MainPage.todos.Save();
                await MainPage.todos.Load();
            }
            App.rootFrame.Navigate(typeof(MainPage));
        };
        var scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
#if DESKTOP || WINDOWS
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
#else
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
#endif

            Content = all,
        };
        Ti.Toggled += (s, e) =>
        {
            time.disabled = !(bool)Ti.IsOn;
            date.disabled = !(bool)Ti.IsOn;
        };
        this.Loaded += (s, e) =>
        {
            time.disabled = !(bool)Ti.IsOn;
            date.disabled = !(bool)Ti.IsOn;
            var bounds = App.MainWindow.Bounds;
            all.Width = this.ActualWidth;
            space.Height = new GridLength(this.ActualHeight / 9.2, GridUnitType.Pixel);
            all.Spacing = this.ActualHeight / 96;
            times.Spacing = all.Spacing;
#if DESKTOP || WINDOWS
            back.Height = this.ActualHeight / 17.3;
#else
            if (bounds.Width > bounds.Height)
            {
                back.Height = this.ActualHeight / 8.88;
            }
            else back.Height = this.ActualHeight / 17.1;
#endif

            divide.Width = this.ActualWidth;
            divide.Height = this.ActualHeight / 676.6;
            title.Height = back.Height * 1.1;
            title.Width = title.Height * 2.3;
            title.Margin = new Thickness((this.ActualWidth / 6.597677), all.Spacing * 4.8, 0, 0);
            title.FontSize = title.Width / 4.6;


#if ANDROID
            if (bounds.Width > bounds.Height)
            {
                describe.Height = this.ActualHeight / 1.7;
            }
            else
            {
                describe.Height = this.ActualHeight / 3.3;
            }
#elif DESKTOP || WINDOWS
            describe.Height = this.ActualHeight / 3.3;
#endif
            describe.Width = describe.Height * 0.8;
            describe.FontSize = describe.Height / 12.46;
            back.Width = back.Height * 2.3;
            back.Margin = new Thickness((this.ActualWidth / 6.597677), 0, 0, 0);
            back.FontSize = back.Width / 4.8;
            done.Margin = back.Margin;
            done.Width = back.Width;
            done.Height = back.Height;
            done.FontSize = back.FontSize;
            describe.Margin = done.Margin;
            color.Height = done.Height * 0.8;
            color.Width = done.Width;
            color.Margin = done.Margin;
            Ti.Width = done.Width * 0.6;
            Ti.Height = done.Height * 0.59;
            Ti.Margin = done.Margin;
            Co.Width = done.Width * 0.6;
            Co.Height = done.Height * 0.59;
            Co.Margin = done.Margin;
            date.Height = done.Height;
            date.Width = done.Width * 2.76;
            time.Height = done.Height;
            time.Width = done.Width * 2.22;
            time.Margin = done.Margin;
            date.Margin = done.Margin;
        };
        this.SizeChanged += (s, e) =>
        {
            time.disabled = !(bool)Ti.IsOn;
            date.disabled = !(bool)Ti.IsOn;
            var bounds = App.MainWindow.Bounds;
            all.Width = this.ActualWidth;
            space.Height = new GridLength(this.ActualHeight / 9.2, GridUnitType.Pixel);
            all.Spacing = this.ActualHeight / 96;
            times.Spacing = all.Spacing;
#if DESKTOP || WINDOWS
            back.Height = this.ActualHeight / 17.3;
#else
            if (bounds.Width > bounds.Height)
            {
                back.Height = this.ActualHeight / 8.88;
            }
            else back.Height = this.ActualHeight / 17.1;
#endif

            divide.Width = this.ActualWidth;
            divide.Height = this.ActualHeight / 676.6;
            title.Height = back.Height * 1.1;
            title.Width = title.Height * 2.3;
            title.Margin = new Thickness((this.ActualWidth / 6.597677), all.Spacing * 4.8, 0, 0);
            title.FontSize = title.Width / 4.6;


#if ANDROID
            if (bounds.Width > bounds.Height)
            {
                describe.Height = this.ActualHeight / 1.7;
            }
            else
            {
                describe.Height = this.ActualHeight / 3.3;
            }
#elif DESKTOP || WINDOWS
            describe.Height = this.ActualHeight / 3.3;
#endif
            describe.Width = describe.Height * 0.8;
            describe.FontSize = describe.Height / 12.46;
            back.Width = back.Height * 2.3;
            back.Margin = new Thickness((this.ActualWidth / 6.597677), 0, 0, 0);
            back.FontSize = back.Width / 4.8;
            done.Margin = back.Margin;
            done.Width = back.Width;
            done.Height = back.Height;
            done.FontSize = back.FontSize;
            describe.Margin = done.Margin;
            color.Height = done.Height * 0.8;
            color.Width = done.Width;
            color.Margin = done.Margin;
            Ti.Width = done.Width * 0.6;
            Ti.Height = done.Height * 0.59;
            Ti.Margin = done.Margin;
            Co.Width = done.Width * 0.6;
            Co.Height = done.Height * 0.59;
            Co.Margin = done.Margin;
            date.Height = done.Height;
            date.Width = done.Width * 2.76;
            time.Height = done.Height;
            time.Width = done.Width * 2.22;
            time.Margin = done.Margin;
            date.Margin = done.Margin;
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
                N.Background = new SolidColorBrush(Colors.White);
                times.BorderBrush = new SolidColorBrush(Colors.Black);
            }
            else if (App.ThemeMode == "Dark")
            {
                N.Background = new SolidColorBrush(Colors.Black);
                times.BorderBrush = new SolidColorBrush(Colors.White);
            }
        };
        if (App.ThemeMode == "Light")
        {
            N.Background = new SolidColorBrush(Colors.White);
            times.BorderBrush = new SolidColorBrush(Colors.Black);
        }
        else if (App.ThemeMode == "Dark")
        {
            N.Background = new SolidColorBrush(Colors.Black);
            times.BorderBrush = new SolidColorBrush(Colors.White);
        }
        Helpers.Add(N, scroll, 1, 0);
        this.Content = N;
    }
}


partial class UpDownBox : UserControl
{
    public string? Text;
    public RepeatButton? up;
    public RepeatButton? down;
    public TextBox? text;
    private bool _value = false;
    public event Action<bool> OnChanged;
    public bool disabled
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnChanged?.Invoke(_value);
            }
        }
    }

    public UpDownBox()
    {
        var c = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                (text = new TextBox
                {
                    IsReadOnly = true,
                    IsEnabled = !disabled,
                }),
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        (up = new RepeatButton
                        {
                            Content = "+",
                            Delay = 105,
                            Interval = 35,
                            IsEnabled = !disabled
                        }),
                        (down = new RepeatButton
                        {
                            Content = "\u2212",
                            Delay = 105,
                            Interval = 35,
                            IsEnabled = !disabled
                        })
                    }
                }
            }

        };
        this.OnChanged += (s) =>
        {
            text.IsEnabled = !disabled;
            up.IsEnabled = !disabled;
            down.IsEnabled = !disabled;
        };
        this.Content = c;
        this.SizeChanged += (s, e) =>
        {
            c.Spacing = 0;
            ((TextBox)c.Children[0]).Height = this.Height;
            ((TextBox)c.Children[0]).Width = this.Width * (14.0 / 30.0);
            ((TextBox)c.Children[0]).FontSize = ((TextBox)c.Children[0]).Width / 6.49;
            ((StackPanel)c.Children[1]).Spacing = 0;
            ((StackPanel)c.Children[1]).Height = this.Height;
            ((StackPanel)c.Children[1]).Width = this.Width - c.Spacing - ((FrameworkElement)c.Children[0]).Width;

            ((RepeatButton)((StackPanel)c.Children[1]).Children[0]).Width = this.Height;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[0]).Height = this.Height;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[0]).FontSize = this.Width * 0.13;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[1]).Width = this.Height;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[1]).Height = this.Height;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[1]).FontSize = this.Width * 0.13;
        };
        this.Loaded += (s, e) =>
        {
            text.Text = this.Text;
            c.Spacing = 0;
            ((TextBox)c.Children[0]).Height = this.Height;
            ((TextBox)c.Children[0]).Width = this.Width * (14.0 / 30.0);
            ((TextBox)c.Children[0]).FontSize = ((TextBox)c.Children[0]).Width / 6.49;
            ((StackPanel)c.Children[1]).Spacing = 0;
            ((StackPanel)c.Children[1]).Height = this.Height;
            ((StackPanel)c.Children[1]).Width = this.Width - c.Spacing - ((FrameworkElement)c.Children[0]).Width;

            ((RepeatButton)((StackPanel)c.Children[1]).Children[0]).Width = this.Height;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[0]).Height = this.Height;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[0]).FontSize = this.Width * 0.13;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[1]).Width = this.Height;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[1]).Height = this.Height;
            ((RepeatButton)((StackPanel)c.Children[1]).Children[1]).FontSize = this.Width * 0.13;
        };
    }
}

public sealed partial class CPwindow : Flyout
{
    public ColorPicker? cp = new ColorPicker
    {
        IsColorSliderVisible = true,
        IsColorChannelTextInputVisible = true,
        IsHexInputVisible = true,
        IsMoreButtonVisible = true,
        ColorSpectrumShape = ColorSpectrumShape.Box,
        IsAlphaEnabled = true,
        IsAlphaSliderVisible = true,
        IsAlphaTextInputVisible = true
    };
    public CPwindow()
    {
        if (App.ThemeMode == "Light")
        {
            cp.Background = new SolidColorBrush(Colors.White);

        }
        else if (App.ThemeMode == "Dark")
        {
            cp.Background = new SolidColorBrush(Colors.Black);
        }
        this.Content = cp;
    }
}

public sealed partial class CPWbutton : Button
{
    public CPwindow cpw = new CPwindow();
    public Rectangle? rect = new();
    public CPWbutton()
    {

        this.Click += (s, e) =>
        {
            cpw = new CPwindow();
            cpw.Placement = FlyoutPlacementMode.TopEdgeAlignedRight;
            cpw.ShowAt(App.rootFrame);
            cpw.cp.ColorChanged += (s, e) =>
            {
                rect.Fill = new SolidColorBrush(cpw.cp.Color);
            };
        };
        this.Content = rect;
        this.SizeChanged += (s, e) =>
        {
            rect.Width = this.ActualWidth;
            rect.Height = this.ActualHeight;
        };
        this.Loaded += (s, e) =>
        {
            rect.Width = this.ActualWidth;
            rect.Height = this.ActualHeight;
        };
    }
}
