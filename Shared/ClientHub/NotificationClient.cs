using Abeer.Shared.EventNotification;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abeer.Shared.ClientHub
{
    public class NotificationClient : IAsyncDisposable
    {
        private string HubUrl { get; set; }
        private string Token { get; set; }
        private HubConnection _hubConnection;
        public List<Notification> Notifications { get; set; }
        private bool _started = false;
        public NotificationClient(string hubUrl, string token = "")
        {
            HubUrl = hubUrl;
            Notifications = new List<Notification>();
            Token = token;
        }
        public async Task StartAsync()
        {
            if (!_started)
            {
                List<TimeSpan> x = new List<TimeSpan>();
                x.Add(new TimeSpan(0, 0, 1000));
                x.Add(new TimeSpan(0, 0, 5000));
                x.Add(new TimeSpan(0, 0, 10000));

                _hubConnection = new HubConnectionBuilder()
                    .WithAutomaticReconnect(x.ToArray())
                    .WithUrl(HubUrl, options =>
                    {
                        options.AccessTokenProvider = async () =>
                        {
                            return await Task.Run(() => Token);
                        };
                    })
                    .Build();

                _hubConnection.On<Notification>("OnNotification", (notification) =>
                {
                    NotificationHandle(notification);
                }); 

                // start the connection
                await _hubConnection.StartAsync();
                _started = true;
            }
        }
        private void NotificationHandle(Notification notification)
        {
            NotificationEvent?.Invoke(this, new NotificationEventArgs(notification));
        }
        public delegate void NotificationEventHandler(object sender, NotificationEventArgs e);
        public event NotificationEventHandler NotificationEvent;

        public async Task SendNotifications(List<Notification> notifs)
        {
            foreach (Notification item in notifs)
            {
                switch (item.NotificationType)
                {
                    case "soonexpireprofile":
                        await _hubConnection.SendAsync("InvokeSoonExpireProfil", item);
                        break;
                    case "daily-reminder":
                        await _hubConnection.SendAsync("InvokeDailyReminder", item);
                        break;
                    default:
                        break;
                }

            }
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
        public async Task StopAsync(bool doubleConnection = false)
        {
            if (_started)
            {
                // disconnect the client
                await _hubConnection.StopAsync();
                // There is a bug in the mono/SignalR client that does not
                // close connections even after stop/dispose
                // see https://github.com/mono/mono/issues/18628
                // this means the demo won't show "xxx left the chat" since 
                // the connections are left open
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _started = false;
            }
        }
    }

}
