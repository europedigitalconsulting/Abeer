using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abeer.Shared.Functional;

namespace Abeer.Shared.ViewModels
{
    public class CountryViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Gdp { get; set; }
        public string Faocode { get; set; }
        public string Label { get; set; }
        public string Eeacode { get; set; }
        public string Estatcode { get; set; }
        public string Nutscode { get; set; }
        public string Culture { get; set; }
        public bool Selected { get; set; }

        public static implicit operator CountryViewModel(Country model)
        {
            return new CountryViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Gdp = model.Gdp,
                Faocode = model.Faocode,
                Label = model.Label,
                Eeacode = model.Eeacode,
                Estatcode = model.Estatcode,
                Nutscode = model.Nutscode,
                Culture = model.Culture
            };
    }

        public static explicit operator Country(CountryViewModel model)
        {
            return new Country
            {
                Id = model.Id,
                Name = model.Name,
                Gdp = model.Gdp,
                Faocode = model.Faocode,
                Label = model.Label,
                Eeacode = model.Eeacode,
                Estatcode = model.Estatcode,
                Nutscode = model.Nutscode,
                Culture = model.Culture
            };
        }
    }
     
}
