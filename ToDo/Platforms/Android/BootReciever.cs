using System.IO;
using System.Text.Json;
using Android.App;
using Android.Content;
using System.Threading.Tasks;
using static ToDo.ToDos;
using Path = System.IO.Path;

namespace ToDo.Droid;

[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted })]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == Intent.ActionBootCompleted)
        {
            PendingResult? pendingResult = GoAsync();

            Task.Run(async () =>
            {
                try
                {
                    List<ToDos.ToDo> ts = new();
                    string folderPath = Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                        "ToDo");

                    Directory.CreateDirectory(folderPath);

                    string filePath = Path.Combine(folderPath, "todos.json");

                    if (!File.Exists(filePath))
                    {
                        await File.WriteAllTextAsync(filePath, "[]");
                    }

                    string jsonData = await File.ReadAllTextAsync(filePath);
                    var todos = JsonSerializer.Deserialize<List<ToDoData>>(jsonData);

                    if (todos == null)
                        return;

                    foreach (var d in todos)
                    {
                        ts.Add(
                            new ToDos.ToDo(
                                d.Title ?? "",
                                d.Descrip ?? "",
                                d.Date,
                                d.Time,
                                d.ID));
                    }

                    INotificationService notifs = new AndroidNotificationService();

                    List<int> todelete = new List<int>();
                    int i = 0;
                    foreach (var todo in ts)
                    {
                        if (todo.Time != null)
                        {
                            if (todo.Date == null || string.IsNullOrEmpty(todo.ID))
                                continue;

                            DateTime scheduledTime = todo.Date.Value.ToDateTime(todo.Time.Value);

                            if (scheduledTime > DateTime.Now)
                            {
                                notifs.CancelNotification(todo.ID);
                                notifs.ScheduleNotification(todo.Title, todo.Descrip, new DateTimeOffset(scheduledTime), todo.ID);
                            }
                            else
                            {
                                todelete.Add(i);
                            }
                        }
                        i++;
                    }
                    for (int j = 0; j < todelete.Count; j++)
                    {
                        await ts[todelete[j]].Delete();
                    }
                    List<ToDoData> todoS = new();

                    foreach (var t in ts)
                    {
                        if (t == null) continue;

                        todoS.Add(new ToDoData
                        {
                            Title = t.Title,
                            Descrip = t.Descrip,
                            Date = t.Date,
                            Time = t.Time,
                            ID = t.ID
                        });
                    }

                    string jsondata = JsonSerializer.Serialize(todoS);

                    string folderpath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ToDo");

                    Directory.CreateDirectory(folderPath);

                    string filepath = Path.Combine(folderPath, "todos.json");

                    await File.WriteAllTextAsync(filepath, jsondata);
                    Console.WriteLine("ToDos saved");
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[BootReceiver Error]: {ex.Message}");
                }
                finally
                {
                    pendingResult.Finish();
                }
            });
        }
    }
}
