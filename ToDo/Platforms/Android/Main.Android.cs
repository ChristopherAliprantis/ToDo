using System;
using Android.App;
using Android.Runtime;

namespace ToDo.Droid;

[Application(
    Label = "ToDo",
    Icon = "@mipmap/appicon",
    LargeHeap = true,
    HardwareAccelerated = true,
    Theme = "@style/Theme.App.Starting"
)]
public class Application : Microsoft.UI.Xaml.NativeApplication
{
    public static INotificationService NotificationService { get; private set; }
    public Application(IntPtr javaReference, JniHandleOwnership transfer)
    : base(() => new App(), javaReference, transfer)
    {
        NotificationService = new AndroidNotificationService();
    }
}
