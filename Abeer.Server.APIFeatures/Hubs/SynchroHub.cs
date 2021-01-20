using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Abeer.Server.APIFeatures.Hubs
{
    public class SynchroHub :Hub
    {
        public override async Task OnConnectedAsync()
        {
            string clientId = Context.GetHttpContext().Request.Headers["ClientId"];
            string groupName = Context.GetHttpContext().Request.Headers["GroupName"];

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Others.SendAsync("Joined", clientId);
        }
    }
}
