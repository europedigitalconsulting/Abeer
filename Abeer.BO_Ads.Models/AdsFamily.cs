using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Abeer.Ads.Models
{
    public class AdsFamily
    {
        public AdsFamily()
        {
            Attributes = new List<AdsFamilyAttribute>();
            Categories = new List<AdsCategory>();
            FamilyId = Guid.NewGuid();
        }

        [Key]
        public Guid FamilyId { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public string PictureUrl { get; set; }
        public string LabelSearch { get; set; }
        public string HeaderAuthorize { get; set; }
        public string UrlApi { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public ICollection<AdsFamilyAttribute> Attributes { get; set; }
        public ICollection<AdsCategory> Categories { get; set; }
        public string PurchaseLabelRule { get; set; }
    }
}