using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.StateContainer
{
    public class StateTchatContainer
    {
        public bool ModalChatContactOpen { get; set; } 
        public bool ModalChatOpen { get; set; }
        public ViewContact ContactSelected { get; set; }
        public List<ViewContact> MyContacts { get; set; } = new List<ViewContact>(); 
        public List<Message> ListMessage { get; set; } = new List<Message>();
        public event Action OnChange;

        public void SetModalChatContactOpen(bool value)
        {
            ModalChatContactOpen = value;
            NotifyStateChanged();
        }
        public void SetModalTchat(bool value)
        {
            ModalChatOpen = value;
            if (!value) 
                ContactSelected = null;

            NotifyStateChanged();
        } 
        public void SetMessage(Message value)
        {
            ListMessage.Add(value);
            NotifyStateChanged();
        }
        public void SetMessage(List<Message> value)
        {
            ListMessage.AddRange(value);
            NotifyStateChanged();
        }
        public void SetMyContacts(ViewContact value)
        {
            MyContacts.Add(value);
            NotifyStateChanged();
        }
        public void SetMyContacts(List<ViewContact> value)
        {
            if (value != null && value.Count > 0)
            {
                MyContacts.AddRange(value);
                NotifyStateChanged();
            }
        }
        public List<Message> GetListMessage()
        {
            return ListMessage.Where(x => (x.UserIdFrom.ToString() == ContactSelected.OwnerId && x.UserIdTo.ToString() == ContactSelected.UserId.ToString())
|| (x.UserIdTo.ToString() == ContactSelected.OwnerId && x.UserIdFrom.ToString() == ContactSelected.UserId.ToString())).ToList();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
