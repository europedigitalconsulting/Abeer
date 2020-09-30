using System;

namespace Abeer.Shared
{
    public class Message
    {
        public long Id { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string ReadUrl { get; set; }
        public string CssClass { get; set; }
        public UserInfo From { get; set; }
    }

    public  class UserInfo
    {
        public long Id { get; set; }
        public string ImageUrl { get; set; }
        public string DisplayName { get; set; }
        public string IconClass { get; set; }
        public bool IsOnline { get; set; }
    }
}
