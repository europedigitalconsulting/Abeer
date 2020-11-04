using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
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
        public ClaimsPrincipal User { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var response = await httpClient.GetAsync("api/Profile");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            UserProfile = JsonConvert.DeserializeObject<ViewApplicationUser>(json);
        }

        protected override async Task OnParametersSetAsync()
        {
            NavigationUrlService.SetUrls($"https://www.google.com/maps/search/?api=1&query={UserProfile.Address},{UserProfile.City}%20{UserProfile.Country}&query_place_id={UserProfile.DisplayName}",
                NavigationManager.ToAbsoluteUri($"/ImportContact/{UserProfile.Id}").ToString());

            var authState = await authenticationStateTask;

            User = authState.User;

            if (User.Identity.IsAuthenticated)
            {
                NavigationUrlService.ShowContacts = true;
                NavigationUrlService.ShowMyAds = true;

                if (User.FindFirstValue(ClaimTypes.NameIdentifier).Equals(UserProfile.Id))
                {
                    NavigationUrlService.ShowImport = false;
                    NavigationUrlService.ShowEditProfile = true;
                }
            }
        }
    }
}