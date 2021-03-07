using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Functional
{
    public class EventTrackingItem
    {
        public EventTrackingItem()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Key { get; set; }
        public string Category { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
