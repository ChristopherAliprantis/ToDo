using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace ToDo.Droid;

[Activity(
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
    private const int NotificationPermissionRequestCode = 1000;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        global::AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications)
                != Permission.Granted)
            {
                RequestPermissions(
                    new[] { Android.Manifest.Permission.PostNotifications },
                    NotificationPermissionRequestCode
                );
            }
        }
    }

    public override void OnRequestPermissionsResult(
        int requestCode,
        string[] permissions,
        Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == NotificationPermissionRequestCode)
        {
            if (grantResults.Length > 0 &&
                grantResults[0] == Permission.Granted)
            {
                // Notifications allowed
            }
            else
            {
                // Notifications denied
            }
        }
    }
}
