using System;
using System.Collections.Generic;

namespace Abeer.Ads.Shared
{
    public class AdsFamilyViewModel
    {
        public Guid FamilyId { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public string PurchaseLabelRule { get; set; }
        public string PictureUrl { get; set; }
        public string LabelSearch { get; set; }
        public string HeaderAuthorize { get; set; }
        public string UrlApi { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public List<AdsFamilyAttributeViewModel> Attributes { get; set; }
        public List<AdsCategoryViewModel> Categories { get; set; }
        
    }
}
