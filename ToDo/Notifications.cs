using System;
using System.Threading.Tasks;

namespace ToDo;

public static class Notifications
{
    public static async Task SendNotif(ToDos.ToDo todo)
    {
        if (todo.Date == null || todo.Time == null || string.IsNullOrEmpty(todo.ID)) return;
        DateTime scheduledTime = todo.Date.Value.ToDateTime(todo.Time.Value);
        await Task.Run(() => { App.NotificationService?.ScheduleNotification(todo.Title, todo.Descrip, new DateTimeOffset(scheduledTime), todo.ID); });
        
    }

    public static void CancelNotif(ToDos.ToDo todo)
    {
        App.NotificationService.CancelNotification(todo.ID);
    }
}
