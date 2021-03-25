using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using AutoMapper;

namespace Abeer.Ads.Models
{
    public class AutoMapping : Profile
    {
        public override string ProfileName => "Ads";

        public AutoMapping()
        {
            CreateMap<AdsFamily, AdsFamilyViewModel>().ReverseMap();
            CreateMap<AdsFamilyAttribute, AdsFamilyAttributeViewModel>().ReverseMap();
            CreateMap<AdsCategory, AdsCategoryViewModel>().ReverseMap();
            CreateMap<CategoryAd, CategoryAdViewModel>().ReverseMap();
        }
    }
}
