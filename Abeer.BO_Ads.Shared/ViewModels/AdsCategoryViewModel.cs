using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abeer.Ads.Shared
{
    public class AdsCategoryViewModel
    { 
        public bool Selected { get; set; }
        public Guid CategoryId { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string PictureUrl { get; set; }
        public Guid FamilyId { get; set; }  
        public AdsFamilyViewModel Family { get; set; }
    }
}