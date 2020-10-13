using System;
using System.Buffers;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared.Functional
{
    public class OfferModel
    {
        public OfferModel()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Url1 { get; set; }
        public string Url2 { get; set; }
        public string Url3 { get; set; }
        public string Url4 { get; set; }
        public string OwnerId { get; set; }
        public int ViewCount { get; set; }
        public OfferPrice OfferPrice { get; set; }
        public string PaymentInformation { get; set; }
        public DateTime StartDisplayTime { get; set; }
        public DateTime? EndDisplayTime { get; set; }
        public bool IsValid { get; set; }
        public DateTime ValidateDate { get; set; }
    }

    public class OfferPrice
    {
        public OfferPrice()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public string PriceName { get; set; }
        public string PriceDescription { get; set; }
        public decimal Value { get; set; }
        public int DelayToDisplay { get; set; }
        public int? DisplayDuration { get; set; }
        public int MaxViewCount { get; set; }
    }
}
