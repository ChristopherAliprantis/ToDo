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

    public void ScheduleNotification(
        string title,
        string message,
        DateTimeOffset scheduleTime,
        string actionData)
    {
        long fileTime =
            scheduleTime.UtcDateTime.ToFileTimeUtc();

        ScheduleToast(
            actionData,
            title,
            message,
            fileTime);
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
