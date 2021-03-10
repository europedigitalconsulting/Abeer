using Abeer.Shared.Functional;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Server.APIFeatures.Hubs
{
    public interface INotificationHub
    {
        Task OnNotification(Notification notification);
    }

    public interface INotificationHubInvokeMethods
    {
        Task InvokeDailyReminder(Notification notification);
        Task InvokeSoonExpireProfil(Notification notification);
    }

    public class NotificationHub : Hub<INotificationHub>, INotificationHubInvokeMethods
    {
        public async Task InvokeDailyReminder(Notification notification)
        {
            var connectionId = Context.ConnectionId;
            await Clients.Client(connectionId).OnNotification(notification);
        }
        public async Task InvokeSoonExpireProfil(Notification notification)
        {
            var connectionId = Context.ConnectionId;
            await Clients.Client(connectionId).OnNotification(notification); 
        }
    }
}
