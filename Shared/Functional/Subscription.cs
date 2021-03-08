using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Functional
{
    public class Subscription
    {
        public Subscription()
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.UtcNow;
        }
        [Key]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string UserId { get; set; }
        public Guid SubscriptionPackId { get; set; }
        [ForeignKey(nameof(SubscriptionPackId))]
        public SubscriptionPack SubscriptionPack { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsValidated { get; set; }
        public Guid? PaymentId { get; set; }
        [ForeignKey(nameof(PaymentId))]
        public PaymentModel Payment { get; set; }
        public bool Enable { get; set; }
    }
}
