using System.Transactions;

namespace Abeer.Shared
{
    public class NodeInformation
    {
        public string ConnectionId { get; set; }
        public string ClientId { get; set; }
        public string ClientUrl { get; set; }

        public NodeInformation()
        {

        }

        public NodeInformation(string connectionId, string clientId, string clientUrl)
        {
            ConnectionId = connectionId;
            ClientId = clientId;
            ClientUrl = clientUrl;
        }
    }
}
