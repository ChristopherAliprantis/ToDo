using System;
using System.Collections.Generic;
using System.Text;
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
                })

            }

        };
        back.Click += (s, e) =>
        {
            App.rootFrame.Navigate(typeof(MainPage));
        };
        done.Click += (s, e) =>
        {
            MainPage.todos.ADD(title.Text, describe.Text);
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
            back.Width = back.Height * 2.3;
            back.Margin = new Thickness((this.ActualWidth / 6.597677), 0, 0, 0);
            back.FontSize = back.ActualWidth / 4.8;
            divide.Width = this.ActualWidth;done.Margin = back.Margin;
            done.Width = back.Width;
            done.Height = back.Height;
            done.FontSize = back.FontSize;
            divide.Height = this.ActualHeight / 676.6;
            title.Height = back.Height * 1.1;
            title.Width = title.Height * 2.3;
            title.Margin = new Thickness((this.ActualWidth / 6.597677), all.Spacing * 4.8, 0, 0);
            title.FontSize = title.ActualWidth / 4.6;
            
            describe.Margin = done.Margin;
            describe.Margin = done.Margin;
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
            describe.Margin = done.Margin;
        };
        Helpers.Add(N, scroll, 1, 0);
        this.Content = N;
    }
}

