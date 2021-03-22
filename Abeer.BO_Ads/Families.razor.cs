using System;
using System.Collections.Generic;
using System.Security.Claims;
using Abeer.Ads.Shared;
using Abeer.Shared.Technical;
using Abeer.Shared.ViewModels;
using Abeer.UI.Crud;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;
using Abeer.AttributeEditor;

namespace Abeer.Ads
{
    public partial class Families : CrudComponentBase<AdsFamilyViewModel>
    {
        protected override string ApiUrl => "/api/bo/Families";

        protected override Guid GetId(AdsFamilyViewModel item) => item.FamilyId;

        protected override bool IsAllowed(ClaimsPrincipal user, string rule) => User?.HasClaim(ClaimTypes.Role, "admin") == true;

        protected override bool IsEquals(AdsFamilyViewModel i, AdsFamilyViewModel m) => m.FamilyId == i.FamilyId;

        protected override bool IsMatch(AdsFamilyViewModel m) =>
            (m.Code != null && m.Code.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
            (m.Label != null && m.Label.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase));

        private List<EditAttributeViewModel> Attributes { get; set; }

        private List<EditAttributeViewModel> GetAttributes(AdsFamilyViewModel family)
        {
            if (family?.Attributes?.Any() == true)
                Attributes = new List<EditAttributeViewModel>(family.Attributes.Cast());
            else
                Attributes = new List<EditAttributeViewModel>();

            Console.WriteLine($"Attributes : {Attributes.Count}");
            return Attributes;
        }

        protected override void PreInsert(AdsFamilyViewModel family)
        {
            family.Attributes = Attributes.Select(a => (AdsFamilyAttributeViewModel)a).ToList();
        }

        protected override void PreUpdate(AdsFamilyViewModel family)
        {
            family.Attributes = Attributes.Select(a => (AdsFamilyAttributeViewModel)a).ToList();
        }
    }
}