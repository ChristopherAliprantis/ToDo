using System;
using System.Threading.Tasks;

namespace ToDo;

public static class Notifications
{
    public static async Task SendNotif(ToDos.ToDo todo)
    {
        if (todo.Date == null || todo.Time == null || string.IsNullOrEmpty(todo.ID)) return;
        DateTime scheduledTime = todo.Date.Value.Date + todo.Time.Value.ToTimeSpan();

        if (scheduledTime > DateTime.Now)
        {
            // Passes the 'todo.ID' as actionData
            App.NotificationService?.ScheduleNotification(todo.Title, todo.Descrip, new DateTimeOffset(scheduledTime), todo.ID);
        }
        await Task.CompletedTask;
    }

    public static async Task CancelNotif(string todoId)
    {
        App.NotificationService?.CancelNotification(todoId);
        await ToDos.ToDo.DeleteById(todoId);
    }

}
