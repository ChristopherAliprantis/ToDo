using System;
using Android.App;
using Android.Content;
using Android.OS;
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

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            actionData.GetHashCode(),
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable
        );

        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

        alarmManager.SetExactAndAllowWhileIdle(
            AlarmType.RtcWakeup,
            scheduleTime.ToUnixTimeMilliseconds(),
            pendingIntent
        );
    }

    public void CancelNotification(string actionData)
    {
        var context = Application.Context;

        var intent = new Intent(context, typeof(NotificationReceiver));

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            actionData.GetHashCode(),
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable
        );

        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
        alarmManager.Cancel(pendingIntent);
    }

    // 👇 REQUIRED for background delivery
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var title = intent.GetStringExtra("title") ?? "Reminder";
            var message = intent.GetStringExtra("message") ?? "";
            var data = intent.GetStringExtra("action_data") ?? "default";

            const string channelId = "todo_channel";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    channelId,
                    "ToDo Reminders",
                    NotificationImportance.High
                );

                var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                manager.CreateNotificationChannel(channel);
            }

            var clickIntent = new Intent(context, typeof(MainActivity));
            clickIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
            clickIntent.PutExtra("action_data", data);

            var clickPending = PendingIntent.GetActivity(
                context,
                data.GetHashCode(),
                clickIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable
            );

            var builder = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetContentIntent(clickPending)
                .SetAutoCancel(true);

            NotificationManagerCompat.From(context)
                .Notify(data.GetHashCode(), builder.Build());
        }
    }
}
