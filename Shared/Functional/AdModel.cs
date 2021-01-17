using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abeer.Shared.Functional
{
    public class AdModel
    {
        public AdModel()
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string Url1 { get; set; }
        public string Url2 { get; set; }
        public string Url3 { get; set; }
        public string Url4 { get; set; }
        public string ImageUrl1 { get; set; }
        public string ImageUrl2 { get; set; }
        public string ImageUrl3 { get; set; }
        public string ImageUrl4 { get; set; }
        public string OwnerId { get; set; }
        public int ViewCount { get; set; }
        public string PaymentInformation { get; set; }
        public DateTime StartDisplayTime { get; set; }
        public DateTime? EndDisplayTime { get; set; }
        public bool IsValid { get; set; }
        public DateTime ValidateDate { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid AdPriceId { get; set; }
        [ForeignKey(nameof(AdPriceId))]
        public AdPrice AdPrice { get; set; }
        public  string Country { get; set; }
    }
}
