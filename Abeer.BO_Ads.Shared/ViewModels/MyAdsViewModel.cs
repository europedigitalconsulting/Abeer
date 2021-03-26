using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;

namespace Abeer.Ads.Shared
{
    public class MyAdsViewModel
    {
        public List<AdsFamilyViewModel> Families { get; set; } 
        public List<CountryViewModel> Countries { get; set; } 
        public List<AdViewModel> Ads { get; set; } 
        public List<string> ListCodeCountrySelected { get; set; }
        public List<Guid> ListIdCategorySelected { get; set; }
        public string searchTxt { get; set; }

    }

}
