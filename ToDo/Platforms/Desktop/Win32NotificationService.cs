using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Path = System.IO.Path;

namespace ToDo.Win32;

public partial class Win32NotificationService : global::ToDo.INotificationService
{
    private string IconPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icons", "todoico.ico");

    public static partial class Imports
    {
        [LibraryImport(
            "Assets/DLLs/WinRTapis.dll",
            StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
        public static partial void ShowToast(
            string title,
            string message);

        [LibraryImport(
            "Assets/DLLs/WinRTapis.dll",
            StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
        public static partial void ScheduleToast(
            string id,
            string title,
            string message,
            long fileTime);

        [LibraryImport(
            "Assets/DLLs/WinRTapis.dll",
            StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
        public static partial void CancelToast(
            string id);
    }

    public void ShowImmediate(string title, string message)
    {
        Imports.ShowToast(title, message);
    }

    static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        var sb = new System.Text.StringBuilder(s.Length);

        foreach (char c in s)
        {
            sb.Append(
                (c >= 0x20 || c == '\t' || c == '\n' || c == '\r')
                ? c
                : ' ');
        }

        return sb.ToString();
    }

    public void ScheduleNotification(
        string title,
        string message,
        DateTimeOffset scheduleTime,
        string actionData)
    {
        var minTime = DateTimeOffset.UtcNow.AddSeconds(60);

        if (scheduleTime < minTime)
            scheduleTime = minTime;

        long fileTime = scheduleTime.UtcDateTime.ToFileTimeUtc();

        title = Sanitize(title);
        message = Sanitize(message);

        if (string.IsNullOrEmpty(actionData))
            actionData = string.Empty;

        if (actionData.Length > 63)
            actionData = actionData.Substring(0, 63);

        Imports.ScheduleToast(
            actionData,
            title,
            message,
            fileTime);
    }

    public void CancelNotification(string actionData)
    {
        Console.WriteLine($"Canceling notification with actionData: '{actionData}'");
        Imports.CancelToast(actionData);
    }
}
