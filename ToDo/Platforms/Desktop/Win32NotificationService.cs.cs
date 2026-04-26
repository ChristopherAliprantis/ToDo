using System;
using System.Diagnostics;
using Microsoft.Win32;
using Path = System.IO.Path; // Explicit alias to avoid Shape collision

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
            // Registers 'ToDo' so the Toast header and Action Center show the correct name
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\AppUserModelId\ToDo");
            key.SetValue("DisplayName", "ToDo");

            // Reference to your icon from the csproj assets
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "icons", "todoico.ico");
            if (System.IO.File.Exists(iconPath)) key.SetValue("IconUri", iconPath);
        }
        catch { }
    }

    public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
    {
        // 'actionData' is your ToDo ID; used here to name the signal file
        string signalPath = Path.Combine(Path.GetTempPath(), $"todo_click_{actionData}.txt");

        // PowerShell script to trigger a native Windows Toast with 'ToDo' branding
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

        if (scheduleTime <= DateTimeOffset.Now)
        {
            RunPs(toastScript);
        }
        else
        {
            // Real OS scheduling via Windows Task Scheduler
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
