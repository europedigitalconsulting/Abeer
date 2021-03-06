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
        Task InvokeSendNotification(Notification notification);
    }

    public class NotificationHub : Hub<INotificationHub>, INotificationHubInvokeMethods
    {
        public async Task InvokeSendNotification(Notification notification)
        {
            await Clients.Others.OnNotification(notification);
        }
    }
}
