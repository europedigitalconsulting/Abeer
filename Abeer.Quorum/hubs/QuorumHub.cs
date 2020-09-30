using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cryptocoin.Quorum.hubs
{
    [Authorize]
    public class QuorumHub : Hub
    {
        private readonly ILogger<QuorumHub> logger;

        public QuorumHub(ILogger<QuorumHub> logger)
        {
            this.logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();

            string clientId = Context.User.FindFirstValue("client_id");
            string groupName = context.Request.Headers["group"];
            string clientUrl = context.Request.Headers["clientUrl"];

            logger.LogInformation($"start connect client {clientId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.OthersInGroup(groupName).SendAsync("Join", Context.ConnectionId, clientId, clientUrl);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string clientId = Context.User.FindFirstValue("client_id");
            string groupName = Context.GetHttpContext().Request.Headers["group"];
            string clientUrl = Context.GetHttpContext().Request.Headers["clientUrl"];

            logger.LogInformation($"start disconnect client {clientId}");


            if (exception != null)
                logger.LogError(exception, $"client {Context.User.FindFirstValue("ClientId")} has occurred an error {exception}");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.OthersInGroup(groupName).SendAsync("Leave", Context.ConnectionId, clientId, clientUrl);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Join(string connectionId, string clientId, string clientUrl)
        {
            var context = Context.GetHttpContext();
            string groupName = context.Request.Headers["group"];

            logger.LogInformation($"start join method with client {clientId}");
            await Clients.OthersInGroup(groupName).SendAsync("Join", connectionId, clientId, clientUrl);
        }
    }
}