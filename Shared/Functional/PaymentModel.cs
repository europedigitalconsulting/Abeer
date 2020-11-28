using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abeer.Shared.Functional
{
    public class PaymentModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? AdId { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string UserId { get; set; }
        public string PaymentReference { get; set; }
        public string PaymentMethod { get; set; }
        public string TokenId { get; set; }
        public bool IsValidated { get; set; }
        public DateTime ValidatedDate { get; set; }
        public string PayerID { get; set; }
    }
}
