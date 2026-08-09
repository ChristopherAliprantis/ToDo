using Microsoft.UI.Xaml.Media.Animation;
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
            var bounds = App.MainWindow.Bounds;
            double size = 0;
#if DESKTOP || WINDOWS
            size = ActualHeight / 27.0;
#else
            if (bounds.Width > bounds.Height == false) size = ActualHeight / 23.0;
            else size = ActualHeight / 8.0;
            arrowpic.Margin = new Thickness(0, 0, 0, size * 2);
#endif
            back.Width = size;
            back.Height = size;

            arrowpic.Width = size;
            arrowpic.Height = size;
        }

        SizeChanged += (s, e) =>
        {
            var bounds = App.MainWindow.Bounds;
            ResizeButton();
            theme.Spacing = ActualWidth / 15.0;
            ((TextBlock)theme.Children[0]).Height = ActualHeight / 15.0;
            ((TextBlock)theme.Children[0]).FontSize = ((TextBlock)theme.Children[0]).Height / 2.0;
            theme.Width = ActualWidth;
            theme.Height = ActualHeight / 15.0;
            THEME.Height = theme.Height;
            if (bounds.Width > bounds.Height == false) THEME.Width = THEME.Height * 2.8;
            else THEME.Width = THEME.Height * 4;
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
            var bounds = App.MainWindow.Bounds;
            ResizeButton();
            theme.Spacing = ActualWidth / 15.0;
            ((TextBlock)theme.Children[0]).Height = ActualHeight / 15.0;
            ((TextBlock)theme.Children[0]).FontSize = ((TextBlock)theme.Children[0]).Height / 2.0;
            theme.Width = ActualWidth;
            theme.Height = ActualHeight / 15.0;
            THEME.Height = theme.Height;
            if (bounds.Width > bounds.Height == false) THEME.Width = THEME.Height * 2.8;
            else THEME.Width = THEME.Height * 4;
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
        App.Themes.ThemeChanged += async(s, e) =>
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
                S.Background = new SolidColorBrush(Colors.White);
            }
            else if (App.ThemeMode == "Dark")
            {
                S.Background = new SolidColorBrush(Colors.Black);
            }
        };
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

sealed class Switch : UserControl
{
    private const double DesignWidth = 44;
    private const double DesignHeight = 20;

    private readonly Grid root;
    private readonly Canvas canvas;
    private readonly Border track;
    private readonly Ellipse thumb;
    private readonly CompositeTransform thumbTransform;

    private Storyboard? currentStoryboard;

    private bool isPointerOver;
    private bool isPressed;
    private bool isDragging;
    private bool hasMoved;
    private bool ignoreVisualUpdate;

    private double pressStartX;
    private double currentThumbX;

    private double thumbOffset;
    public event RoutedEventHandler? Toggled;



    public static readonly DependencyProperty IsOnProperty =
        DependencyProperty.Register(
            nameof(IsOn),
            typeof(bool),
            typeof(Switch),
            new PropertyMetadata(false, OnIsOnChanged));



    public static readonly DependencyProperty OnColorProperty =
        DependencyProperty.Register(
            nameof(OnColor),
            typeof(Brush),
            typeof(Switch),
            new PropertyMetadata(
                new SolidColorBrush(
                    Color.FromARGB(
                        255,
                        0,
                        120,
                        212))));



    public static readonly DependencyProperty OffColorProperty =
        DependencyProperty.Register(
            nameof(OffColor),
            typeof(Brush),
            typeof(Switch),
            new PropertyMetadata(
                new SolidColorBrush(
                    Color.FromARGB(
                        255,
                        128,
                        128,
                        128))));



    public static readonly DependencyProperty ThumbColorProperty =
        DependencyProperty.Register(
            nameof(ThumbColor),
            typeof(Brush),
            typeof(Switch),
            new PropertyMetadata(
                new SolidColorBrush(
                    Color.FromARGB(
                        255,
                        255,
                        255,
                        255))));



    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }


    public Brush OnColor
    {
        get => (Brush)GetValue(OnColorProperty);
        set => SetValue(OnColorProperty, value);
    }


    public Brush OffColor
    {
        get => (Brush)GetValue(OffColorProperty);
        set => SetValue(OffColorProperty, value);
    }


    public Brush ThumbColor
    {
        get => (Brush)GetValue(ThumbColorProperty);
        set => SetValue(ThumbColorProperty, value);
    }




    public Switch()
    {
        root = new Grid();

        canvas = new Canvas();

        track = new Border();


        thumbTransform = new CompositeTransform();


        thumb = new Ellipse
        {
            RenderTransform = thumbTransform,
            Shadow = new ThemeShadow()
        };


        canvas.Children.Add(track);
        canvas.Children.Add(thumb);

        root.Children.Add(canvas);

        Content = root;



        PointerEntered += (_, _) =>
        {
            isPointerOver = true;
            UpdateVisual(false);
        };


        PointerExited += (_, _) =>
        {
            isPointerOver = false;
            UpdateVisual(false);
        };



        PointerPressed += (_, e) =>
        {
            if (!IsEnabled)
                return;


            currentStoryboard?.Stop();


            isPressed = true;
            isDragging = false;
            hasMoved = false;


            pressStartX =
                e.GetCurrentPoint(this)
                 .Position.X;


            CapturePointer(e.Pointer);


            SetSquish(true);
        };



        PointerMoved += (_, e) =>
        {
            if (!isPressed)
                return;


            double x =
                e.GetCurrentPoint(this)
                 .Position.X;


            if (Math.Abs(x - pressStartX) > 4)
            {
                hasMoved = true;
                isDragging = true;
            }


            if (isDragging)
            {
                MoveThumb(x);
            }
        };



        PointerReleased += (_, e) =>
        {
            if (!isPressed)
                return;


            isPressed = false;


            ReleasePointerCapture(e.Pointer);


            SetSquish(false);


            ignoreVisualUpdate = false;



            if (!hasMoved)
            {
                IsOn = !IsOn;
            }
            else
            {
                IsOn =
                    thumbOffset >
                    (GetOnPosition() - GetOffPosition()) / 2;
            }



            isDragging = false;


            currentThumbX =
                IsOn
                ? GetOnPosition()
                : GetOffPosition();
        };



        SizeChanged += (_, _) =>
        {
            UpdateVisual(false);
        };


        Loaded += (_, _) =>
        {
            UpdateVisual(false);
        };
    }




    private static void OnIsOnChanged(
        DependencyObject sender,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (Switch)sender;


        if (!control.ignoreVisualUpdate)
        {
            control.UpdateVisual(true);
        }


        control.Toggled?.Invoke(
            control,
            new RoutedEventArgs());
    }




    private void UpdateVisual(bool animate)
    {
        if (ActualWidth <= 0 ||
            ActualHeight <= 0)
            return;



        double scale =
            Math.Min(
                ActualWidth / DesignWidth,
                ActualHeight / DesignHeight);



        double width =
            DesignWidth * scale;


        double height =
            DesignHeight * scale;


        double left =
            (ActualWidth - width) / 2;


        double top =
            (ActualHeight - height) / 2;



        double thumbSize =
            15 * scale;


        double padding =
            2 * scale;



        track.Width = width;
        track.Height = height;

        track.CornerRadius =
            new CornerRadius(height / 2);


        track.Background =
            IsOn
            ? OnColor
            : OffColor;


        Canvas.SetLeft(track, left);
        Canvas.SetTop(track, top);



        thumb.Width = thumbSize;
        thumb.Height = thumbSize;

        thumb.Fill =
            ThumbColor;


        thumb.Stroke =
            new SolidColorBrush(
                Color.FromARGB(
                    40,
                    0,
                    0,
                    0));


        thumb.StrokeThickness =
            scale;


        Canvas.SetLeft(
            thumb,
            GetOffPosition());

        Canvas.SetTop(
            thumb,
            top + ((height - thumbSize) / 2));



        double target =
            IsOn
            ? GetOnPosition()
            : GetOffPosition();


        thumbOffset =
            target - GetOffPosition();



        if (!isDragging)
        {
            currentThumbX = target;

            if (animate)
            {
                AnimateThumb(target);
            }
            else
            {
                thumbTransform.TranslateX = thumbOffset;
            }
        }



        if (!IsEnabled)
            Opacity = 0.4;
        else if (isPressed)
            Opacity = 0.75;
        else if (isPointerOver)
            Opacity = 0.9;
        else
            Opacity = 1;
    }




    private void MoveThumb(double pointerX)
    {
        ignoreVisualUpdate = true;


        double x =
            Math.Clamp(
                pointerX - thumb.Width / 2,
                GetOffPosition(),
                GetOnPosition());


        thumbOffset =
            x - GetOffPosition();


        thumbTransform.TranslateX =
            thumbOffset;


        SetSquish(true);
    }



    private double GetOffPosition()
    {
        double scale =
            Math.Min(
                ActualWidth / DesignWidth,
                ActualHeight / DesignHeight);


        double width =
            DesignWidth * scale;


        double left =
            (ActualWidth - width) / 2;


        return left + (2 * scale);
    }




    private double GetOnPosition()
    {
        double scale =
            Math.Min(
                ActualWidth / DesignWidth,
                ActualHeight / DesignHeight);


        double width =
            DesignWidth * scale;


        double thumbSize =
            15 * scale;


        double left =
            (ActualWidth - width) / 2;


        return left +
               width -
               thumbSize -
               (2 * scale);
    }




    private void AnimateThumb(double target)
    {
        currentStoryboard?.Stop();

        currentStoryboard =
            new Storyboard();


        double offset =
            target - GetOffPosition();


        var animation =
            new DoubleAnimation
            {
                From =
                    thumbTransform.TranslateX,

                To = offset,

                Duration =
                    new Duration(
                        TimeSpan.FromMilliseconds(
                            160)),

                EasingFunction =
                    new CubicEase()
            };
    }

    private double GetThumbTravel()
    {
        return GetOnPosition() - GetOffPosition();
    }


    private void SetSquish(bool value)
    {
        thumbTransform.ScaleX =
            value ? 1.08 : 1;


        thumbTransform.ScaleY =
            value ? 0.92 : 1;
    }
}
