using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;

namespace ToDo.Win32;

public class Win32NotificationService : INotificationService
{
    private string Safe(string id)
        => id.Replace(" ", "_");

    private string IconPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "icons", "todoico.ico");

    public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
    {
        string id = Safe(actionData);

        string ps =
            $"powershell -WindowStyle Hidden -Command \"& {{ " +
            $"New-BurntToastNotification -Text '{title}', '{message}' " +
            $"-AppLogo '{IconPath}' " +
            $"}}\"";

        string st = scheduleTime.ToString("HH:mm");
        string sd = scheduleTime.ToString("MM/dd/yyyy");

        Process.Start(new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            Arguments = $"/Create /SC ONCE /TN \"ToDo_Notif_{id}\" /TR \"{ps}\" /ST {st} /SD {sd} /F",
            UseShellExecute = true,
            CreateNoWindow = true
        });
    }

    public void CancelNotification(string actionData)
    {
        string id = Safe(actionData);

        Process.Start(new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            Arguments = $"/Delete /TN \"ToDo_Notif_{id}\" /F",
            UseShellExecute = true,
            CreateNoWindow = true
        });
    }
}
