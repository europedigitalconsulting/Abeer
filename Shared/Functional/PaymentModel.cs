using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abeer.Shared.Functional
{
    public class PaymentModel
    {
        public PaymentModel()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public Guid? Reference { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public string PaymentReference { get; set; }
        public string PaymentMethod { get; set; }
        public string TokenId { get; set; }
        public bool IsValidated { get; set; }
        public DateTime ValidatedDate { get; set; }
        public string PayerID { get; set; }
        public string PaymentType { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalTTc { get; set; }
        public string NoteToPayer { get; set; }
    }
}
