using System;
using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace ToDo.Droid;

public class AndroidNotificationService : INotificationService
{
    public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
    {
        var context = Application.Context;
        var intent = new Intent(context, typeof(NotificationReceiver));
        intent.PutExtra("title", title);
        intent.PutExtra("message", message);
        intent.PutExtra("action_data", actionData);

        var pending = PendingIntent.GetBroadcast(context, actionData.GetHashCode(), intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

        // Using SetAlarmClock is the 'High Priority' way to wake the phone
        var info = new AlarmManager.AlarmClockInfo(scheduleTime.ToUnixTimeMilliseconds(), pending);
        alarmManager.SetAlarmClock(info, pending);
    }

    public void CancelNotification(string actionData)
    {
        var context = Application.Context;
        var intent = new Intent(context, typeof(NotificationReceiver));
        var pending = PendingIntent.GetBroadcast(context, actionData.GetHashCode(), intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
        alarmManager.Cancel(pending);
    }
}

[BroadcastReceiver(Enabled = true, Exported = true)]
public class NotificationReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        var title = intent.GetStringExtra("title");
        var message = intent.GetStringExtra("message");
        var data = intent.GetStringExtra("action_data");
        var channelId = "urgent_reminders";

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            // Set to High Importance so it pops up (peeks) at the user
            var channel = new NotificationChannel(channelId, "Urgent Reminders", NotificationImportance.High);
            ((NotificationManager)context.GetSystemService(Context.NotificationService)).CreateNotificationChannel(channel);
        }

        // Setup the intent so clicking the notification opens the app
        var clickIntent = new Intent(context, typeof(MainActivity));
        clickIntent.PutExtra("action_data", data);
        var pending = PendingIntent.GetActivity(context, 0, clickIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var builder = new NotificationCompat.Builder(context, channelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetContentIntent(pending)
            .SetAutoCancel(true);

        NotificationManagerCompat.From(context).Notify(data.GetHashCode(), builder.Build());
    }
}
