using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Server.APIFeatures.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {

        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
    public interface INotificationHub
    {
        Task OnNotification(Notification notification);
    }

    public interface INotificationHubInvokeMethods
    {
        Task InvokeDailyReminder(Notification notification);
        Task InvokeSoonExpireProfil(Notification notification);
        Task InvokeAddContact(Notification notification, string userId);
    }
    [Authorize]
    public class NotificationHub : Hub<INotificationHub>, INotificationHubInvokeMethods
    {
        public async Task InvokeDailyReminder(Notification notification)
        {
            var userId = Context.UserIdentifier;
            await Clients.User(userId).OnNotification(notification);
        }
        public async Task InvokeSoonExpireProfil(Notification notification)
        {
            var userId = Context.UserIdentifier;
            await Clients.User(userId).OnNotification(notification);
        }
        public async Task InvokeAddContact(Notification notification, string userId)
        {
            await Clients.User(userId).OnNotification(notification);
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
