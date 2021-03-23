using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Abeer.Ads.Models
{
    public class AdsFamilyAttribute
    {
        [Key]
        public Guid FamilyAttributeId { get; set; }
        public Guid FamilyId { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSearchable { get; set; }
        public string Type{ get; set; }
        [ForeignKey(nameof(FamilyId))]
        public AdsFamily Family { get; set; }
    }
}
