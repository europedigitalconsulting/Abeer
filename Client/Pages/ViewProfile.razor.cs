using Abeer.Shared;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class ViewProfile : ComponentBase
    {
        public ClaimsPrincipal User { get; set; }

        [Parameter]
        public string ProfileId { get; set; }

        public ViewApplicationUser UserProfile { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected override Task OnInitializedAsync()
        {
            NavigationUrlService.ProfileUrl = Navigation.ToAbsoluteUri("/profile/Edit").ToString();
            NavigationUrlService.ProfileId = ProfileId;
            return base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            var authState = await authenticationStateTask;

            User = authState.User;

            var httpClient = HttpClientFactory.CreateClient(Configuration["Service:Api:AnonymousApiName"]);

            if (User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(ProfileId))
                {
                    ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
                {
                    Category = "Navigation",
                    Key = $"ViewProfile#{ProfileId}",
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                });
            }
            else
            {
                await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
                {
                    Category = "Navigation",
                    Key = $"ViewProfile#{ProfileId}",
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid()
                });
            }

            var apiUrl = $"/api/viewProfile/{ProfileId}";
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("social", out var _social))
            {
                apiUrl += $"?social={_social}";
            }

            var getProfile = await httpClient.GetAsync(apiUrl);

            if (getProfile.StatusCode == System.Net.HttpStatusCode.NotFound)
                Navigation.NavigateTo(Navigation.ToAbsoluteUri($"/Identity/Account/Register?PinCode={ProfileId}").ToString(), true);

            getProfile.EnsureSuccessStatusCode();
            var json = await getProfile.Content.ReadAsStringAsync();
            UserProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<ViewApplicationUser>(json);

            NavigationUrlService.SetUrls($"https://www.google.com/maps/search/?api=1&query={UserProfile.Address},{UserProfile.City}%20{UserProfile.Country}&query_place_id={UserProfile.DisplayName}",
                Navigation.ToAbsoluteUri($"/contact/import/{UserProfile.Id}").ToString());


            NavigationUrlService.ShowImport = true;

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

            await InvokeAsync(StateHasChanged);
        }
    }
}
