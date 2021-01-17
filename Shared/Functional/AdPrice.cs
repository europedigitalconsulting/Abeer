using System;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared.Functional
{
    public class AdPrice
    {
        public AdPrice()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public string PriceName { get; set; }
        public string PriceDescription { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; }
        public int DelayToDisplay { get; set; }
        public int? DisplayDuration { get; set; }
        public int MaxViewCount { get; set; }
    }
}