using System;
using System.IO;
using System.Diagnostics;
using Path = System.IO.Path;

namespace ToDo.Win32;

public class Win32NotificationService : global::ToDo.INotificationService
{
    private string IconPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icons", "todoico.ico");

    [DllImport(
        "Assets/DLLs/WinRTapis.dll",
        CallingConvention = CallingConvention.StdCall,
        CharSet = CharSet.Unicode)]
    public static extern void ShowToast(
        string title,
        string message);
    public void ShowImmediate(string title, string message)
    {
        ShowToast(title, message);
    }

    [DllImport("Assets/DLLs/WinRTapis.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Unicode)]
    public static extern void ScheduleToast(
    string id,
    string title,
    string message,
    long fileTime);

    static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char c in s) sb.Append((c >= 0x20 || c == '\t' || c == '\n' || c == '\r') ? c : ' ');
        return sb.ToString();
    }

    public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
    {
        // ensure at least 60s in future to avoid timing validation issues
        var minTime = DateTimeOffset.UtcNow.AddSeconds(60);
        if (scheduleTime < minTime) scheduleTime = minTime;

        long fileTime = scheduleTime.UtcDateTime.ToFileTimeUtc();

        // ensure non-empty content (fallback message)
        title = Sanitize(title);
        message = Sanitize(message);
        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(message))
            message = "Scheduled notification";

        // ensure small tag, or pass empty so native side won't set Tag
        if (string.IsNullOrEmpty(actionData)) actionData = string.Empty;
        if (actionData.Length > 63) actionData = actionData.Substring(0, 63);

        ScheduleToast(actionData, title, message, fileTime);
    }

    [DllImport("Assets/DLLs/WinRTapis.dll",
        CallingConvention = CallingConvention.StdCall,
        CharSet = CharSet.Unicode)]
    public static extern void CancelToast(string id);
    public void CancelNotification(string actionData)
    {
        CancelToast(actionData);
    }
}
