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
            await Task.Run(()=> {App.NotificationService?.ScheduleNotification(todo.Title, todo.Descrip, new DateTimeOffset(scheduledTime), todo.ID); });
        }
    }

    public static async Task CancelNotif(string todoId)
    {
        await ToDos.ToDo.DeleteById(todoId);
    }

}
