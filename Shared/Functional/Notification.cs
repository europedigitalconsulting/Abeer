using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Functional
{
    public class Notification
    {
        public Notification()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string NotificationType { get; set; }
        public string NotificationIcon { get; set; }
        public string NotificationUrl { get; set; }
        public bool IsDisplayOnlyOnce { get; set; }
        public bool IsDisplayed { get; set; }
        public int DisplayMax { get; set; }
        public int DisplayCount { get; set; }
        public string MessageTitle { get; set; }
        public DateTime LastDisplayTime { get; set; }
        public string CssClass { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
