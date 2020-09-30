using System;

namespace Abeer.Shared
{
    public class Notification
    {
        public long Id { get; set; }
        public string ReadUrl { get; set; }
        public string CssClass { get; set; }
        public string ImageUrl { get; set; }
        public string IconClass { get; set; }
        public string Title { get; set; }
        public DateTime ReceivedDate { get; set; }
    }
}
