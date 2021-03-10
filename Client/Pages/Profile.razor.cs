using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class Profile : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient httpClient { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }

        public ViewApplicationUser UserProfile { get; set; } = new ViewApplicationUser();
        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();
        public ClaimsPrincipal User { get; set; }
        public bool ReadOnly { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var authState = await authenticationStateTask;

            User = authState.User;

            if (User.Identity.IsAuthenticated)
            {
                var response = await httpClient.GetAsync("api/Profile/GetUserProfileNoDetail");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    UserProfile = JsonConvert.DeserializeObject<ViewApplicationUser>(json);

                    NavigationUrlService.SetUrls($"https://www.google.com/maps/search/?api=1&query={UserProfile.Address},{UserProfile.City}%20{UserProfile.Country}&query_place_id={UserProfile.DisplayName}",
                        NavigationManager.ToAbsoluteUri($"/contact/import/{UserProfile.Id}").ToString());

                    NavigationUrlService.ShowContacts = true;
                    NavigationUrlService.ShowMyAds = true;

                    if (User.FindFirstValue(ClaimTypes.NameIdentifier).Equals(UserProfile.Id))
                    {
                        NavigationUrlService.ShowImport = false;
                        NavigationUrlService.ShowEditProfile = true;
                    }

                    var responseSocialNetwork = await httpClient.GetAsync("api/socialnetwork");
                    response.EnsureSuccessStatusCode();

                    var jsonSocialNetwork = await responseSocialNetwork.Content.ReadAsStringAsync();
                    AvailableSocialNetworks = JsonConvert.DeserializeObject<List<SocialNetwork>>(jsonSocialNetwork);
                }
            }
        }
    }
}