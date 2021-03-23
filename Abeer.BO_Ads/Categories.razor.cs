using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using Abeer.UI.Crud;
using Newtonsoft.Json;

namespace Abeer.Ads
{
    public partial class Categories : HierarchyCrudComponentBase<AdsCategoryViewModel, AdsFamilyViewModel>
    {
        protected override string ApiBaseUrl => "/api/bo/Categories";

        protected override Func<AdsFamilyViewModel, string> ApiGetUrl =>
            family => $"{ApiBaseUrl}/byfamily/{family.FamilyId}";

        protected override string ParentApiUrl => "/api/bo/Families";

        protected override bool ShowHierarchy => true;
        protected override Func<AdsFamilyViewModel, string> ParentDisplayText => fam => LocFamily[fam.Label];
        protected override IList<AdsCategoryViewModel> FilterHierarchy()
        {
            return Data.Where(c => c.FamilyId == Parent.FamilyId).ToList();
        }

        protected override Guid GetId(AdsCategoryViewModel item) => item.CategoryId;

        protected override bool IsAllowed(ClaimsPrincipal user, string rule) => User?.HasClaim(ClaimTypes.Role, "admin") == true;

        protected override bool IsEquals(AdsCategoryViewModel i, AdsCategoryViewModel m) => m.CategoryId == i.CategoryId;

        protected override bool IsMatch(AdsCategoryViewModel m) =>
            (m.Code != null && m.Code.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
            (m.Label != null && m.Label.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase));

        protected override void PreInsert(AdsCategoryViewModel item)
        {
            item.FamilyId = Parent.FamilyId;
        }

        protected override string ParentUrl => "/bo/Categories";
    }
}