using System;

namespace Abeer.Ads.Shared
{
    public class AdsCategoryViewModel
    {
        public Guid CategoryId { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string PictureUrl { get; set; }
        public Guid FamilyId { get; set; }
        public  virtual  AdsFamilyViewModel Family { get; set; }
    }
}