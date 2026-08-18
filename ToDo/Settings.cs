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
#if WINDOWS || DESKTOP
            HorizontalAlignment = HorizontalAlignment.Left,
#else
            HorizontalAlignment = HorizontalAlignment.Center,
#endif
            VerticalAlignment = VerticalAlignment.Top,
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
            else size = ActualHeight / 7.0;
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
#if ANDROID
            if (bounds.Width > bounds.Height == false) theme.Height = ActualHeight / 15.0;
            else theme.Height = ActualHeight / 10.0;
#else
            theme.Height = ActualHeight / 15.0;
#endif
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
#if ANDROID
            if (bounds.Width > bounds.Height == false) theme.Height = ActualHeight / 15.0;
            else theme.Height = ActualHeight / 10.0;
#else
            theme.Height = ActualHeight / 15.0;
#endif
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

public sealed class Switch : Microsoft.UI.Xaml.Controls.UserControl
{
    private const double DesignWidth = 44;
    private const double DesignHeight = 20;
    private const double DragThreshold = 4;

    private readonly Microsoft.UI.Xaml.Controls.Grid root;
    private readonly Microsoft.UI.Xaml.Controls.Canvas canvas;
    private readonly Microsoft.UI.Xaml.Controls.Border track;
    private readonly Microsoft.UI.Xaml.Shapes.Ellipse thumb;
    private readonly Microsoft.UI.Xaml.Media.CompositeTransform thumbTransform;

    private Microsoft.UI.Xaml.Media.Animation.Storyboard? currentStoryboard;

    private bool isPointerOver;
    private bool isPressed;
    private bool isDragging;
    private bool hasMoved;
    private bool isInitialized;

    private uint? activePointerId;

    private double pressStartX;
    private double dragProgress;

    public event Microsoft.UI.Xaml.RoutedEventHandler? Toggled;

    public static readonly Microsoft.UI.Xaml.DependencyProperty IsOnProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(IsOn),
            typeof(bool),
            typeof(Switch),
            new Microsoft.UI.Xaml.PropertyMetadata(
                false,
                OnIsOnChanged));

    public static readonly Microsoft.UI.Xaml.DependencyProperty OnColorProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(OnColor),
            typeof(Microsoft.UI.Xaml.Media.Brush),
            typeof(Switch),
            new Microsoft.UI.Xaml.PropertyMetadata(
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(
                        255,
                        0,
                        120,
                        212))));

    public static readonly Microsoft.UI.Xaml.DependencyProperty OffColorProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(OffColor),
            typeof(Microsoft.UI.Xaml.Media.Brush),
            typeof(Switch),
            new Microsoft.UI.Xaml.PropertyMetadata(
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(
                        255,
                        128,
                        128,
                        128))));

    public static readonly Microsoft.UI.Xaml.DependencyProperty ThumbColorProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(ThumbColor),
            typeof(Microsoft.UI.Xaml.Media.Brush),
            typeof(Switch),
            new Microsoft.UI.Xaml.PropertyMetadata(
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(
                        255,
                        255,
                        255,
                        255))));

    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public Microsoft.UI.Xaml.Media.Brush OnColor
    {
        get => (Microsoft.UI.Xaml.Media.Brush)GetValue(OnColorProperty);
        set => SetValue(OnColorProperty, value);
    }

    public Microsoft.UI.Xaml.Media.Brush OffColor
    {
        get => (Microsoft.UI.Xaml.Media.Brush)GetValue(OffColorProperty);
        set => SetValue(OffColorProperty, value);
    }

    public Microsoft.UI.Xaml.Media.Brush ThumbColor
    {
        get => (Microsoft.UI.Xaml.Media.Brush)GetValue(ThumbColorProperty);
        set => SetValue(ThumbColorProperty, value);
    }

    public Switch()
    {
        root = new Microsoft.UI.Xaml.Controls.Grid();

        canvas = new Microsoft.UI.Xaml.Controls.Canvas();

        track = new Microsoft.UI.Xaml.Controls.Border();

        thumbTransform =
            new Microsoft.UI.Xaml.Media.CompositeTransform();

        thumb =
            new Microsoft.UI.Xaml.Shapes.Ellipse
            {
                RenderTransform = thumbTransform,
                Shadow =
                    new Microsoft.UI.Xaml.Media.ThemeShadow()
            };

        canvas.Children.Add(track);
        canvas.Children.Add(thumb);

        root.Children.Add(canvas);

        Content = root;

        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;

        PointerCanceled += OnPointerCanceled;
        PointerCaptureLost += OnPointerCaptureLost;

        SizeChanged += OnSizeChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(
        object sender,
        Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        isInitialized = true;

        dragProgress = IsOn ? 1 : 0;

        UpdateVisual(false);
    }

    private void OnSizeChanged(
        object sender,
        Microsoft.UI.Xaml.SizeChangedEventArgs e)
    {
        UpdateVisual(false);
    }

    private void OnPointerEntered(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        isPointerOver = true;

        UpdateOpacity();
    }

    private void OnPointerExited(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        isPointerOver = false;

        UpdateOpacity();
    }

    private void OnPointerPressed(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!IsEnabled)
            return;

        if (activePointerId.HasValue)
            return;

        var point = e.GetCurrentPoint(this);

        activePointerId = point.PointerId;

        currentStoryboard?.Stop();
        currentStoryboard = null;

        isPressed = true;
        isDragging = false;
        hasMoved = false;

        pressStartX = point.Position.X;

        double travel = GetThumbTravel();

        if (travel > 0)
        {
            dragProgress =
                Math.Clamp(
                    thumbTransform.TranslateX / travel,
                    0,
                    1);
        }
        else
        {
            dragProgress = IsOn ? 1 : 0;
        }

        CapturePointer(e.Pointer);

        SetSquish(true);

        UpdateOpacity();

        e.Handled = true;
    }

    private void OnPointerMoved(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!isPressed ||
            !activePointerId.HasValue)
        {
            return;
        }

        var point = e.GetCurrentPoint(this);

        if (point.PointerId != activePointerId.Value)
            return;

        double x = point.Position.X;

        if (!hasMoved &&
            Math.Abs(x - pressStartX) > DragThreshold)
        {
            hasMoved = true;
            isDragging = true;
        }

        if (!isDragging)
            return;

        MoveThumb(x);

        e.Handled = true;
    }

    private void OnPointerReleased(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!isPressed ||
            !activePointerId.HasValue)
        {
            return;
        }

        var point = e.GetCurrentPoint(this);

        if (point.PointerId != activePointerId.Value)
            return;

        FinishPointerInteraction();

        e.Handled = true;
    }

    private void OnPointerCanceled(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!isPressed ||
            !activePointerId.HasValue)
        {
            return;
        }

        var point = e.GetCurrentPoint(this);

        if (point.PointerId != activePointerId.Value)
            return;

        CancelPointerInteraction();

        e.Handled = true;
    }

    private void OnPointerCaptureLost(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!isPressed)
            return;

        CancelPointerInteraction();
    }

    private void FinishPointerInteraction()
    {
        bool wasDragging = isDragging;
        bool wasMoved = hasMoved;

        bool newState;

        if (!wasMoved)
        {
            newState = !IsOn;
        }
        else
        {
            newState = dragProgress >= 0.5;
        }

        isPressed = false;
        isDragging = false;
        hasMoved = false;

        try
        {
            ReleasePointerCaptures();
        }
        catch
        {
        }

        activePointerId = null;

        SetSquish(false);

        if (IsOn != newState)
        {
            IsOn = newState;
        }

        dragProgress = IsOn ? 1 : 0;

        UpdateVisual(true);

        UpdateOpacity();
    }

    private void CancelPointerInteraction()
    {
        isPressed = false;
        isDragging = false;
        hasMoved = false;

        try
        {
            ReleasePointerCaptures();
        }
        catch
        {
        }

        activePointerId = null;

        SetSquish(false);

        dragProgress = IsOn ? 1 : 0;

        UpdateVisual(true);

        UpdateOpacity();
    }

    private static void OnIsOnChanged(
        Microsoft.UI.Xaml.DependencyObject sender,
        Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs e)
    {
        var control = (Switch)sender;

        if (!control.isDragging)
        {
            control.currentStoryboard?.Stop();
            control.currentStoryboard = null;

            control.dragProgress =
                control.IsOn ? 1 : 0;

            control.UpdateVisual(false);
        }

        control.Toggled?.Invoke(
            control,
            new Microsoft.UI.Xaml.RoutedEventArgs());
    }

    private void UpdateVisual(bool animate)
    {
        if (ActualWidth <= 0 ||
            ActualHeight <= 0)
        {
            return;
        }

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

        track.Width = width;
        track.Height = height;

        track.CornerRadius =
            new Microsoft.UI.Xaml.CornerRadius(
                height / 2);

        track.Background =
            IsOn
                ? OnColor
                : OffColor;

        Microsoft.UI.Xaml.Controls.Canvas.SetLeft(
            track,
            left);

        Microsoft.UI.Xaml.Controls.Canvas.SetTop(
            track,
            top);

        thumb.Width = thumbSize;
        thumb.Height = thumbSize;

        thumb.Fill = ThumbColor;

        thumb.Stroke =
            new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(
                    40,
                    0,
                    0,
                    0));

        thumb.StrokeThickness = scale;

        Microsoft.UI.Xaml.Controls.Canvas.SetLeft(
            thumb,
            GetOffPosition());

        Microsoft.UI.Xaml.Controls.Canvas.SetTop(
            thumb,
            top + ((height - thumbSize) / 2));

        double travel = GetThumbTravel();

        if (isDragging)
        {
            thumbTransform.TranslateX =
                Math.Clamp(
                    dragProgress,
                    0,
                    1) * travel;
        }
        else
        {
            double target =
                IsOn
                    ? travel
                    : 0;

            if (animate)
            {
                AnimateThumb(target);
            }
            else
            {
                currentStoryboard?.Stop();
                currentStoryboard = null;

                thumbTransform.TranslateX =
                    target;
            }
        }

        UpdateOpacity();
    }

    private void MoveThumb(double pointerX)
    {
        double offPosition =
            GetOffPosition();

        double travel =
            GetThumbTravel();

        if (travel <= 0)
            return;

        double thumbCenter =
            pointerX -
            (thumb.Width / 2);

        double offset =
            thumbCenter -
            offPosition;

        dragProgress =
            Math.Clamp(
                offset / travel,
                0,
                1);

        thumbTransform.TranslateX =
            dragProgress * travel;

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

        return left +
               (2 * scale);
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

    private double GetThumbTravel()
    {
        return Math.Max(
            0,
            GetOnPosition() -
            GetOffPosition());
    }

    private void AnimateThumb(double target)
    {
        currentStoryboard?.Stop();

        currentStoryboard =
            new Microsoft.UI.Xaml.Media.Animation.Storyboard();

        var animation =
            new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From =
                    thumbTransform.TranslateX,

                To =
                    target,

                Duration =
                    new Microsoft.UI.Xaml.Duration(
                        TimeSpan.FromMilliseconds(160)),

                EasingFunction =
                    new Microsoft.UI.Xaml.Media.Animation.CubicEase
                    {
                        EasingMode =
                            Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut
                    }
            };

        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(
            animation,
            thumbTransform);

        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(
            animation,
            "TranslateX");

        currentStoryboard.Children.Add(animation);

        currentStoryboard.Begin();
    }

    private void SetSquish(bool value)
    {
        thumbTransform.ScaleX =
            value ? 1.08 : 1.0;

        thumbTransform.ScaleY =
            value ? 0.92 : 1.0;
    }

    private void UpdateOpacity()
    {
        if (!IsEnabled)
        {
            Opacity = 0.4;
        }
        else if (isPressed)
        {
            Opacity = 0.75;
        }
        else if (isPointerOver)
        {
            Opacity = 0.9;
        }
        else
        {
            Opacity = 1.0;
        }
    }
}
