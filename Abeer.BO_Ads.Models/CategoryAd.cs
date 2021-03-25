using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abeer.Ads.Models
{
    public class CategoryAd
    {
        public CategoryAd()
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }
        public Guid AdId { get; set; }
        public Guid CategoryId { get; set; } 
        public DateTime CreateDate { get; set; } 
    }
}
