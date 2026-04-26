using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace ToDo.Win32
{
    public class Win32NotificationService : INotificationService
    {
        public void ScheduleNotification(string title, string message, DateTimeOffset scheduleTime, string actionData)
        {
            string toastXml = $@"
                <toast launch='{actionData}'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{message}</text>
                        </binding>
                    </visual>
                </toast>";

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(toastXml);

            if (scheduleTime <= DateTimeOffset.Now)
            {
                Show(xmlDoc);
            }
            else
            {
                Task.Run(async () =>
                {
                    var delay = scheduleTime - DateTimeOffset.Now;
                    if (delay.TotalMilliseconds > 0)
                    {
                        await Task.Delay(delay);
                        Show(xmlDoc);
                    }
                });
            }
        }

        private void Show(XmlDocument xml)
        {
            var toast = new ToastNotification(xml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
