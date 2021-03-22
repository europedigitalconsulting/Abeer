using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abeer.Ads.Models
{
    public class AdsCategory
    {
        public AdsCategory()
        {
            CategoryId = Guid.NewGuid();
        }

        [Key]
        public Guid CategoryId { get; set; }  
        public string Code { get; set; }
        public string Label { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string PictureUrl { get; set; }
        [ForeignKey(nameof(FamilyId))]
        public AdsFamily Family { get; set; }
        public  Guid FamilyId { get; set; }
    }
}
