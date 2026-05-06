using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;
using Path = System.IO.Path;

namespace ToDo.Win32;

public class Win32NotificationService : INotificationService
{
    private string IconPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icons", "todoico.ico");

    public void ShowImmediate(string title, string message)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            // FIX: Get the content object first, then the XML string
            string xml = builder.GetToastContent().GetContent();

            SendNative(xml);
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); }
    }

    public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddArgument("id", actionData)
                .AddText(title)
                .AddText(message);

            string xml = builder.GetToastContent().GetContent();

            var doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(xml);

            var toast = new Windows.UI.Notifications.ScheduledToastNotification(doc, scheduleTime)
            {
                Tag = actionData
            };

            Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); }
    }

    private void SendNative(string xml)
    {
        var doc = new Windows.Data.Xml.Dom.XmlDocument();
        doc.LoadXml(xml);
        var toast = new Windows.UI.Notifications.ToastNotification(doc);
        Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(toast);
    }

    public void CancelNotification(string actionData)
    {
        try
        {
            var notifier = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();
            foreach (var n in scheduled)
            {
                if (n.Tag == actionData) notifier.RemoveFromSchedule(n);
            }
        }
        catch { }
    }
}
