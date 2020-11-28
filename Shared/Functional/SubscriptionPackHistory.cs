using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abeer.Shared.Functional
{
    public class SubscriptionHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid SubscriptionPackId { get; set; }
        public bool Enable{ get; set; }
        public DateTime Created { get; set; }
        public DateTime EndSubscription { get; set; }
    }
}
