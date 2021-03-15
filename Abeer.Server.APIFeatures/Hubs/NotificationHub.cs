using Abeer.Data.UnitOfworks;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        Task OnMessageReceived(string text, string userSendId, string contactReceiveId);
    }

    public interface INotificationHubInvokeMethods
    {
        Task InvokeDailyReminder(Notification notification);
        Task InvokeSoonExpireProfil(Notification notification);
        Task InvokeAddContact(Notification notification, string userId);
        Task InvokeSendMessage(string text, string contactReceiveId);
    }
    [Authorize]
    public class NotificationHub : Hub<INotificationHub>, INotificationHubInvokeMethods
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FunctionalUnitOfWork FunctionalUnitOfWork;
        public NotificationHub(FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager)
        {

            FunctionalUnitOfWork = functionalUnitOfWork;
            _userManager = userManager;
        }
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
        public async Task InvokeSendMessage(string text, string contactReceiveId)
        {
            var userSendId = Context.UserIdentifier;
            await Clients.User(contactReceiveId).OnMessageReceived(text, userSendId, contactReceiveId);
        }
         
        public override async Task OnConnectedAsync()
        { 
            var userId = Context.UserIdentifier;
            var ttt = await _userManager.FindByIdAsync(userId);
            if (ttt != null)
            {
                ttt.IsOnline = true;
                await _userManager.UpdateAsync(ttt);
                Contact fff = await FunctionalUnitOfWork.ContactRepository.GetContact(Guid.Parse(ttt.Id));
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception e)
        { 
            var userId = Context.UserIdentifier;
            var ttt = await _userManager.FindByIdAsync(userId);
            ttt.IsOnline = false;
            await _userManager.UpdateAsync(ttt);
            await base.OnDisconnectedAsync(e);
        }
    }
}
