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

            var pendingIntent = PendingIntent.GetBroadcast(context, new Random().Next(), intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, scheduleTime.ToUnixTimeMilliseconds(), pendingIntent);
        }
    }

    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var title = intent.GetStringExtra("title");
            var message = intent.GetStringExtra("message");
            var channelId = "todo_notifications";

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, "To-Do Reminders", NotificationImportance.High);
                ((NotificationManager)context.GetSystemService(Context.NotificationService)).CreateNotificationChannel(channel);
            }

            var builder = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(title) // Becomes Bold/Large
                .SetContentText(message) // Becomes Normal/Small
                .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
                .SetAutoCancel(true);

            NotificationManagerCompat.From(context).Notify(new Random().Next(), builder.Build());
        }
    }
}
