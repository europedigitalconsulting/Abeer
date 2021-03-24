using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Abeer.UI_Ads
{
    public partial class Details : ComponentBase
    {
        [Parameter]
        public  string Id { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        public AuthenticationState AuthenticateSate { get; set; }
        public ViewApplicationUser User { get; set; } = new ViewApplicationUser();

        private AdViewModel Ad { get; set; }

        public int CurrentImageIndex { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            AuthenticateSate = await authenticationStateTask;

            if (AuthenticateSate.User.Identity.IsAuthenticated)
                User = AuthenticateSate.User;

            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);

            var apiUrl = $"/api/ads/{Id}";

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("social", out var _social))
            {
                apiUrl += $"?social={_social}";
            }

            var getDetail = await HttpClient.GetAsync(apiUrl);
            getDetail.EnsureSuccessStatusCode();
            var json = await getDetail.Content.ReadAsStringAsync();
            Ad = JsonConvert.DeserializeObject<AdViewModel>(json);
            await base.OnInitializedAsync();
        }
        public async Task GoToProfilAd()
        {
            NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri($"/viewprofile/{Ad.OwnerId}").ToString(), true);
        }
    }
}
