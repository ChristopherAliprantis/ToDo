using System;
using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace ToDo.Droid
{
    public class AndroidNotificationService : INotificationService
    {
        public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
        {
            var context = Application.Context;
            var intent = new Intent(context, typeof(NotificationReceiver));
            intent.PutExtra("title", title);
            intent.PutExtra("message", message);
            intent.PutExtra("data", actionData);

            var pendingIntent = PendingIntent.GetBroadcast(context, actionData.GetHashCode(), intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, scheduleTime.ToUnixTimeMilliseconds(), pendingIntent);
        }

        public void CancelNotification(string actionData)
        {
            var context = Application.Context;
            var intent = new Intent(context, typeof(NotificationReceiver));
            var pendingIntent = PendingIntent.GetBroadcast(context, actionData.GetHashCode(), intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            ((AlarmManager)context.GetSystemService(Context.AlarmService)).Cancel(pendingIntent);
        }
    }

    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var title = intent.GetStringExtra("title");
            var message = intent.GetStringExtra("message");
            var data = intent.GetStringExtra("data");
            var channelId = "urgent_reminders";

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                // Importance.High makes it pop up (peek) and make sound
                var channel = new NotificationChannel(channelId, "Urgent Reminders", NotificationImportance.High);
                ((NotificationManager)context.GetSystemService(Context.NotificationService)).CreateNotificationChannel(channel);
            }

            var builder = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
                .SetPriority(NotificationCompat.PriorityHigh) // High Priority for older Android versions
                .SetCategory(NotificationCompat.CategoryReminder)
                .SetAutoCancel(true);

            NotificationManagerCompat.From(context).Notify(data.GetHashCode(), builder.Build());
        }
    }
}
