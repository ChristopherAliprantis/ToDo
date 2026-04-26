using System;
using System.Diagnostics;
using System.IO;

namespace ToDo.Win32;

public class Win32NotificationService : INotificationService
{
    public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
    {
        string signalPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"todo_click_{actionData}.txt");

        string toastScript = $"[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null; " +
                             $"$template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02); " +
                             $"$xml = [xml]$template.GetXml(); " +
                             $"$xml.toast.SetAttribute('scenario', 'reminder'); " +
                             $"$xml.toast.SetAttribute('launch', 'cmd.exe /c echo click > \"{signalPath}\"'); " +
                             $"$textNodes = $xml.GetElementsByTagName('text'); " +
                             $"$textNodes.Item(0).AppendChild($xml.CreateTextNode('{title}')) | Out-Null; " +
                             $"$textNodes.Item(1).AppendChild($xml.CreateTextNode('{message}')) | Out-Null; " +
                             $"$toastXml = [Windows.Data.Xml.Dom.XmlDocument]::new(); $toastXml.LoadXml($xml.OuterXml); " +
                             $"$toast = [Windows.UI.Notifications.ToastNotification]::new($toastXml); " +
                             $"[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('ToDoApp').Show($toast);";

        if (scheduleTime <= DateTimeOffset.Now)
        {
            RunPs(toastScript);
        }
        else
        {
            string taskName = $"ToDo_Notif_{actionData.GetHashCode()}";
            string cmd = $"/Create /SC ONCE /TN {taskName} /TR \"powershell -WindowStyle Hidden -Command {toastScript}\" /ST {scheduleTime.ToString("HH:mm")} /SD {scheduleTime.ToString("MM/dd/yyyy")} /F /IT /RL HIGHEST";
            Process.Start(new ProcessStartInfo { FileName = "schtasks.exe", Arguments = cmd, CreateNoWindow = true });
        }
    }

    public void CancelNotification(string actionData)
    {
        Process.Start(new ProcessStartInfo { FileName = "schtasks.exe", Arguments = $"/Delete /TN ToDo_Notif_{actionData.GetHashCode()} /F", CreateNoWindow = true });
    }

    private void RunPs(string s) => Process.Start(new ProcessStartInfo { FileName = "powershell.exe", Arguments = $"-WindowStyle Hidden -Command {s}", CreateNoWindow = true });
}
