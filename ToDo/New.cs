
namespace ToDo;


public sealed partial class New : Page // #if __DESKTOP__ for all of skia desktop, #if WINDOWS for windows, #if __ANDROID__ for android.
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
                    PlaceholderText = "Date"
                }),
                (time = new UpDownBox
                {
                    Visibility = Visibility.Collapsed,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    PlaceholderText = "Time"
                }),

            }

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
            MainPage.todos.ADD(title.Text, describe.Text, null, null);
            App.rootFrame.Navigate(typeof(MainPage));
        };
        var scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
#if __DESKTOP__
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
#if __DESKTOP__
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


#if __ANDROID__
            if (bounds.Width > bounds.Height)
            {
                describe.Height = this.ActualHeight / 1.7;
            }
            else
            {
                describe.Height = this.ActualHeight / 3.3;
            }
#elif __DESKTOP__
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
            date.Width = done.Width * 1.73;
            time.Height = done.Height;
            time.Width = done.Width * 1.73;
            time.Margin = done.Margin;
            date.Margin = done.Margin;
        };
        this.SizeChanged += (s,e) =>
        {
            var bounds = App.MainWindow.Bounds;
            all.Width = this.ActualWidth;
            space.Height = new GridLength(this.ActualHeight / 9.2, GridUnitType.Pixel);
            all.Spacing = this.ActualHeight / 96;
#if __DESKTOP__
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
            
            
#if __ANDROID__
            if (bounds.Width > bounds.Height)
            {
                describe.Height = this.ActualHeight / 1.7;
            }
            else
            {
                describe.Height = this.ActualHeight / 3.3;
            }
#elif __DESKTOP__
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
            date.Width = done.Width * 1.73;
            time.Height = done.Height;
            time.Width = done.Width * 1.73;
            time.Margin = done.Margin;
            date.Margin = done.Margin;
        };
        Helpers.Add(N, scroll, 1, 0);
        this.Content = N;
    }
}


class UpDownBox : UserControl
{
    public string Text = "";
    public string PlaceholderText = "";
    public Button? up;
    public Button? down;

    public UpDownBox()
    {
        var c = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBox
                {
                    IsReadOnly = true,
                    Text = this.Text,
                    PlaceholderText = this.PlaceholderText
                },
                new StackPanel
                {
                    Children =
                    {
                        (up = new Button
                        {
                            Content = "+",
                        }),
                        (down = new Button
                        {
                            Content = "-",
                        })
                    }
                }
            }

        };
        this.Content = c;
        this.SizeChanged += (s, e) =>
        {
            c.Spacing = this.Width / 30;
            ((TextBox)c.Children[0]).Height = this.Height;
            ((TextBox)c.Children[0]).Width = this.Width * (22.0 / 30.0);
            ((TextBox)c.Children[0]).FontSize = ((TextBox)c.Children[0]).Width / 4.6;
            ((StackPanel)c.Children[1]).Spacing = this.Height / 28;
            ((StackPanel)c.Children[1]).Height = this.Height;
            ((StackPanel)c.Children[1]).Width = this.Width - c.Spacing - ((FrameworkElement)c.Children[0]).Width;
            for (int i = 0; i <= ((StackPanel)c.Children[1]).Children.Count; i++)
            {
                ((Button)((StackPanel)c.Children[1]).Children[i]).Height = ((StackPanel)c.Children[1]).Height / 2 - ((StackPanel)c.Children[1]).Spacing;
                ((Button)((StackPanel)c.Children[1]).Children[i]).Width = ((Button)((StackPanel)c.Children[1]).Children[i]).Height;
                ((Button)((StackPanel)c.Children[1]).Children[i]).FontSize = ((Button)((StackPanel)c.Children[1]).Children[i]).Width;
            }
        };
        this.Loaded += (s, e) =>
        {
            c.Spacing = this.Width / 30;
            ((TextBox)c.Children[0]).Height = this.Height;
            ((TextBox)c.Children[0]).Width = this.Width * (22.0 / 30.0);
            ((TextBox)c.Children[0]).FontSize = ((TextBox)c.Children[0]).Width / 4.6;
            ((StackPanel)c.Children[1]).Spacing = this.Height / 28;
            ((StackPanel)c.Children[1]).Height = this.Height;
            ((StackPanel)c.Children[1]).Width = this.Width - c.Spacing - ((FrameworkElement)c.Children[0]).Width;
            for (int i = 0; i <= ((StackPanel)c.Children[1]).Children.Count; i++)
            {
                ((Button)((StackPanel)c.Children[1]).Children[i]).Height = ((StackPanel)c.Children[1]).Height / 2 - ((StackPanel)c.Children[1]).Spacing;
                ((Button)((StackPanel)c.Children[1]).Children[i]).Width = ((Button)((StackPanel)c.Children[1]).Children[i]).Height;
                ((Button)((StackPanel)c.Children[1]).Children[i]).FontSize = ((Button)((StackPanel)c.Children[1]).Children[i]).Width;
            }
        };
    }
}
