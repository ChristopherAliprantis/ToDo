namespace ToDo.Droid;

using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

public class AndroidNotificationService : INotificationService
{
    private int GetId(string id) => id.GetHashCode();
    public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
    {
        var context = Android.App.Application.Context;
        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

        // 1. Android 12+ Safety Check: Redirect if permission is missing
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && !alarmManager.CanScheduleExactAlarms())
        {
            var uri = Android.Net.Uri.Parse($"package:{context.PackageName}");
            var settingsIntent = new Intent(Android.Provider.Settings.ActionRequestScheduleExactAlarm, uri);
            settingsIntent.AddFlags(ActivityFlags.NewTask);

            context.StartActivity(settingsIntent);
            return; // Exit out early to prevent the crash/freeze
        }

        // 2. Build the standard Notification Intent
        var intent = new Intent(context, typeof(NotificationReceiver));
        intent.PutExtra("id", actionData);
        intent.PutExtra("title", title);
        intent.PutExtra("message", message);

        var pending = PendingIntent.GetBroadcast(
            context,
            GetId(actionData),
            intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
        );

        long triggerTimeMs = scheduleTime.ToUnixTimeMilliseconds();

        // 3. Fire the Exact Alarm safely
        alarmManager.SetExactAndAllowWhileIdle(
            AlarmType.RtcWakeup,
            triggerTimeMs,
            pending
        );
    }


public void CancelNotification(string actionData)
    {
        var context = Android.App.Application.Context;

        var intent = new Intent(context, typeof(NotificationReceiver));

        var pending = PendingIntent.GetBroadcast(
            context,
            GetId(actionData),
            intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
        );

        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

        alarmManager.Cancel(pending);
        pending.Cancel();
    }
}
[BroadcastReceiver(Enabled = true, Exported = false)]
public class NotificationReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        var title = intent.GetStringExtra("title") ?? "ToDo";
        var message = intent.GetStringExtra("message") ?? "You have a task";
        var id = intent.GetStringExtra("id") ?? "";

        const string channelId = "todo_channel";

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                channelId,
                "ToDo",
                NotificationImportance.High
            );

            var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
        }

        var clickIntent = new Intent(context, typeof(MainActivity));
        clickIntent.PutExtra("id", id);

        var clickPending = PendingIntent.GetActivity(
            context,
            0,
            clickIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
        );

        var builder = new NotificationCompat.Builder(context, channelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
            .SetContentIntent(clickPending)
            .SetAutoCancel(true);

        NotificationManagerCompat.From(context)
            .Notify(id.GetHashCode(), builder.Build());
    }
}
