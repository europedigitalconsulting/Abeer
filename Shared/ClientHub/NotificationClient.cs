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
                        options.CloseTimeout = TimeSpan.FromSeconds(360);
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
                _hubConnection.On<string, string, string>("OnMessageReceived", (text, userIdFrom, userIdTo) =>
                 {
                     MessageReceivedHandle(text, userIdFrom, userIdTo);
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
        private void MessageReceivedHandle(string text, string userIdFrom, string userIdTo)
        {
            MessageReceivedEvent?.Invoke(this, new MessageReceivedEventArgs(text, userIdFrom, userIdTo));
        }
        private void ModalCloseChatHandle()
        {
            ModalCloseChatEvent?.Invoke(this);
        }
        public delegate void NotificationEventHandler(object sender, NotificationEventArgs e);
        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public delegate void ModalCloseChatEventHandler(object sender);
        public event NotificationEventHandler NotificationEvent;
        public event MessageReceivedEventHandler MessageReceivedEvent;
        public event ModalCloseChatEventHandler ModalCloseChatEvent;

        public async Task SendNotifications(List<Notification> notifs)
        {
            await SendingNotifications(notifs);
        }
        public async Task SendNotifications(Notification notif)
        {
            await SendingNotifications(new List<Notification> { notif });
        }
        public async Task SendNotificationsToUser(Notification notif, string userId)
        {
            await SendingNotifications(new List<Notification> { notif }, userId);
        }
        public async Task CloseModalContactTchat()
        {
            await _hubConnection.SendAsync("InvokeCloseModalContactTchat");
        }
        public async Task SendMessage(string text, string userIdTo)
        {
            await _hubConnection.SendAsync("InvokeSendMessage", text, userIdTo);
        }

        private async Task SendingNotifications(List<Notification> notifs, string userId = null)
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
                    case "add-contact":
                        await _hubConnection.SendAsync("InvokeAddContact", item, userId);
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
