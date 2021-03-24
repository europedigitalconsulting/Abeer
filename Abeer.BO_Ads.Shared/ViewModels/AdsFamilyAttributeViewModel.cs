using Abeer.AttributeEditor;
using System;
using System.Collections.Generic;
using System.Linq; 

namespace Abeer.Ads.Shared
{
    public class AdsFamilyAttributeViewModel
    {
        public Guid FamilyAttributeId { get; set; }
        public Guid FamilyId { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSearchable { get; set; }
        public string Type { get; set; }

        public static implicit operator EditAttributeViewModel(AdsFamilyAttributeViewModel viewModel)
        {
            return new EditAttributeViewModel()
            {
                Code = viewModel.Code,
                Label = viewModel.Label,
                Type = viewModel.Type,
                IsSearchable = viewModel.IsSearchable,
                IsRequired = viewModel.IsRequired,
                ParentId = viewModel.FamilyId,
                Id = viewModel.FamilyAttributeId
            };
        }

        public static explicit operator AdsFamilyAttributeViewModel(EditAttributeViewModel attribute)
        {
            return new AdsFamilyAttributeViewModel
            {
                Code = attribute.Code,
                Label = attribute.Label,
                Type = attribute.Type,
                IsSearchable = attribute.IsSearchable,
                IsRequired = attribute.IsRequired,
                FamilyId = attribute.ParentId,
                FamilyAttributeId = attribute.Id
            };
        }
    }

    public static class FamilyAttributeViewModelExtension
    {
        public static List<EditAttributeViewModel> Cast(this List<AdsFamilyAttributeViewModel> items)
        {
            return items?.Select(v => new EditAttributeViewModel()
            {
                Id = v.FamilyAttributeId,
                Code = v.Code,
                Label = v.Label,
                Type = v.Type,
                IsSearchable = v.IsSearchable,
                IsRequired = v.IsRequired,
                ParentId = v.FamilyId
            })?.ToList();
        }
    }
}
