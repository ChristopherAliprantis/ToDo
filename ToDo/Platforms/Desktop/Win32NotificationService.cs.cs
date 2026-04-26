using System;
using System.Diagnostics;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace ToDo.Win32;

public class Win32NotificationService : INotificationService
{
    public Win32NotificationService()
    {
        RegisterApp();
    }

    private void RegisterApp()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\AppUserModelId\ToDo");
            key.SetValue("DisplayName", "ToDo");
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "icons", "todoico.ico");
            if (System.IO.File.Exists(iconPath)) key.SetValue("IconUri", iconPath);
        }
        catch { }
    }

    public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
    {
        string signalPath = Path.Combine(Path.GetTempPath(), $"todo_click_{actionData}.txt");

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
                             $"[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('ToDo').Show($toast);";

        // Safety: If time is in the next 10 seconds, show it now to avoid Scheduler rejection
        if (scheduleTime <= DateTimeOffset.Now.AddSeconds(10))
        {
            RunPs(toastScript);
        }
        else
        {
            string taskName = $"ToDo_Notif_{actionData.GetHashCode()}";

            // Format time as HH:mm:ss for maximum OS compatibility
            string st = scheduleTime.ToString("HH:mm:ss");
            string sd = scheduleTime.ToString("MM/dd/yyyy");

            // /Z ensures the task is deleted automatically after it runs
            string cmd = $"/Create /SC ONCE /TN {taskName} /TR \"powershell -WindowStyle Hidden -Command {toastScript}\" /ST {st} /SD {sd} /F /IT /RL HIGHEST /Z";

            Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = cmd,
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
    }

    public void CancelNotification(string actionData)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            Arguments = $"/Delete /TN ToDo_Notif_{actionData.GetHashCode()} /F",
            CreateNoWindow = true
        });
    }

    private void RunPs(string s) =>
        Process.Start(new ProcessStartInfo { FileName = "powershell.exe", Arguments = $"-WindowStyle Hidden -Command {s}", CreateNoWindow = true });
}
