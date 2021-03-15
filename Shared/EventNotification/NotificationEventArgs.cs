using Abeer.Shared.Functional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.EventNotification
{
    public class NotificationEventArgs : EventArgs
    {
        public Notification Notification { get; set; }
        public NotificationEventArgs(Notification notification)
        {
            Notification = notification;
        }
    }
    public class MessageReceivedEventArgs : EventArgs
    { 
        public string Text { get; set; }
        public string UserSendId { get; set; }
        public string ContactReceiveId { get; set; }
        public MessageReceivedEventArgs(string text, string userSendId, string contactReceiveId)
        {
            Text = text;
            UserSendId = userSendId;
            ContactReceiveId = contactReceiveId;
        }
    }
}
