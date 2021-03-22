using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace Abeer.UI_Ads
{
    public partial class ListAdsFriends : ComponentBase
    {

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        [Inject]HttpClient httpClient { get; set; }
        public string Term { get; set; }
        public bool IsAdmin { get; set; }

        private List<Abeer.Shared.Functional.AdModel> All = new List<Abeer.Shared.Functional.AdModel>();
        private List<Abeer.Shared.Functional.AdModel> Items = new List<Abeer.Shared.Functional.AdModel>();

        private void countTerm(KeyboardEventArgs e)
        {
            if (Term?.Length > 5)
                Search();
        }

        protected override async Task OnInitializedAsync()
        {
            await GetAll();

            var authenticateSate = await authenticationStateTask;

            if (authenticateSate.User?.Identity.IsAuthenticated == true)
            {
                IsAdmin = (authenticateSate.User.Identity.IsAuthenticated && authenticateSate.User.HasClaim(ClaimTypes.Role, "admin"));
            }
        }

        private async Task GetAll()
        {
            var getAll = await httpClient.GetAsync("/api/Ads/freinds");
            if (getAll.IsSuccessStatusCode)
            {
                var json = await getAll.Content.ReadAsStringAsync();
                All = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Abeer.Shared.Functional.AdModel>>(json);
                if (All != null)
                {
                    Items = All?.ToList();
                }
            }
        }

        private void Search()
        {
            Items = All?.Where(a => (a.Title != null && a.Title.Contains(Term)) || (a.Description != null && a.Description.Contains(Term))).ToList();
        }
    }
}
