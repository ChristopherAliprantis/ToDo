
namespace ToDo;


public sealed partial class New : Page // #if DESKTOP for all of skia desktop, #if WINDOWS for windows, #if ANDROID for android.
{
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
        StackPanel? all;
        Button? back;
        Rectangle? divide;
        TextBox? title;
        Button? done;
        TimeOnly Time = TimeOnly.FromDateTime(DateTime.Now);
        DateTime Date = DateTime.Now.Date;
        TextBox? describe;
        Button? op;
        UpDownBox? date;
        UpDownBox? time;
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
                    HorizontalAlignment = HorizontalAlignment.Left
                }),
                (describe = new TextBox
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    PlaceholderText = "Description",
                    TextWrapping = TextWrapping.Wrap,

                }),
                (divide = new Rectangle
                {
                    Fill = new SolidColorBrush(Color.Black)
                }),
                (op = new Button
                {
                    Content = "optional",
                    HorizontalAlignment = HorizontalAlignment.Left,
                }),
                (date = new UpDownBox
                {
                    Visibility = Visibility.Collapsed,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = Date.ToString("yyyy-MM-dd"),
                }),
                (time = new UpDownBox
                {
                    Visibility = Visibility.Collapsed,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = Time.ToString("hh:mm tt"),
                }),

            }

        };
        time.up.Click += (s, e) =>
        {
            Time = Time.AddMinutes(1);
            time.Text = Time.ToString("hh:mm tt");
            time.text.Text = time.Text;
        };
        time.down.Click += (s, e) =>
        {
            Time = Time.AddMinutes(-1);
            time.Text = Time.ToString("hh:mm tt");
            time.text.Text = time.Text;
        };
        date.up.Click += (s, e) =>
        {
            Date = Date.AddDays(1);
            date.Text = Date.ToString("yyyy-MM-dd");
            date.text.Text = date.Text;
        };
        date.down.Click += (s, e) =>
        {
            Date = Date.AddDays(-1);
            date.Text = Date.ToString("yyyy-MM-dd");
            date.text.Text = date.Text;
        };
        op.Click += (s, e) =>
        {
            if (time.Visibility == Visibility.Collapsed)
                time.Visibility = Visibility.Visible;
            else
                time.Visibility = Visibility.Collapsed;

            if (date.Visibility == Visibility.Collapsed)
                date.Visibility = Visibility.Visible;
            else
                date.Visibility = Visibility.Collapsed;
        };
        back.Click += (s, e) =>
        {
            App.rootFrame.Navigate(typeof(MainPage));
        };
        done.Click += (s, e) =>
        {
            if (date.Visibility == Visibility.Collapsed && time.Visibility == Visibility.Collapsed)
            {
                MainPage.todos.ADD(title.Text, describe.Text, null, null, null);
            }
            else
            {
                string ID = System.Guid.NewGuid().ToString();
                MainPage.todos.ADD(title.Text, describe.Text, Date, Time, ID);

            }
            App.rootFrame.Navigate(typeof(MainPage));
        };
        var scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
#if DESKTOP
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
#else
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
#endif

            Content = all,
        };
        this.Loaded += (s, e) =>
        {
            var bounds = App.MainWindow.Bounds;
            all.Width = this.ActualWidth;
            space.Height = new GridLength(this.ActualHeight / 9.2, GridUnitType.Pixel);
            all.Spacing = this.ActualHeight / 96;
#if DESKTOP
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
#elif DESKTOP
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
            op.Width = done.Width * 0.82;
            op.Height = done.Height * 0.79;
            op.FontSize = op.Width / 5.96;
            op.Margin = new Thickness(done.Margin.Left * 0.87, 0, 0, 0);
            date.Height = done.Height;
            date.Width = done.Width * 2.76;
            time.Height = done.Height;
            time.Width = done.Width * 2.22;
            time.Margin = done.Margin;
            date.Margin = done.Margin;
        };
        this.SizeChanged += (s,e) =>
        {
            var bounds = App.MainWindow.Bounds;
            all.Width = this.ActualWidth;
            space.Height = new GridLength(this.ActualHeight / 9.2, GridUnitType.Pixel);
            all.Spacing = this.ActualHeight / 96;
#if DESKTOP
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
#elif DESKTOP
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
            op.Width = done.Width * 0.82;
            op.Height = done.Height * 0.79;
            op.FontSize = op.Width / 5.96;
            op.Margin = new Thickness(done.Margin.Left * 0.87, 0,0,0);
            date.Height = done.Height;
            date.Width = done.Width * 2.76;
            time.Height = done.Height;
            time.Width = done.Width * 2.22;
            time.Margin = done.Margin;
            date.Margin = done.Margin;
        };
        Helpers.Add(N, scroll, 1, 0);
        this.Content = N;
    }
}


class UpDownBox : UserControl
{
    public string? Text;
    public Button? up;
    public Button? down;
    public TextBox? text;

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
                }),
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        (up = new Button
                        {
                            Content = "+",
                        }),
                        (down = new Button
                        {
                            Content = "\u2212",
                        })
                    }
                }
            }

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

            ((Button)((StackPanel)c.Children[1]).Children[0]).Width = this.Height;
            ((Button)((StackPanel)c.Children[1]).Children[0]).Height = this.Height;
            ((Button)((StackPanel)c.Children[1]).Children[0]).FontSize = this.Width * 0.13;
            ((Button)((StackPanel)c.Children[1]).Children[1]).Width = this.Height;
            ((Button)((StackPanel)c.Children[1]).Children[1]).Height = this.Height;
            ((Button)((StackPanel)c.Children[1]).Children[1]).FontSize = this.Width * 0.13;
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

            ((Button)((StackPanel)c.Children[1]).Children[0]).Width = this.Height;
            ((Button)((StackPanel)c.Children[1]).Children[0]).Height = this.Height;
            ((Button)((StackPanel)c.Children[1]).Children[0]).FontSize = this.Width * 0.13;
            ((Button)((StackPanel)c.Children[1]).Children[1]).Width = this.Height;
            ((Button)((StackPanel)c.Children[1]).Children[1]).Height = this.Height;
            ((Button)((StackPanel)c.Children[1]).Children[1]).FontSize = this.Width * 0.13;
        };
    }
}
