using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.ClientHub;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Abeer.UI_Contacts
{
    public partial class Browse : ComponentBase
    {
        private const string PageName = "Browse";
        private const string PageCategory = "Contact";

        [Inject] private IHttpClientFactory httpClientFactory { get; set; }
        [CascadingParameter]
        public ScreenSize ScreenSize { get; set; }
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        private AuthenticationState authenticateState { get; set; }
        private bool IsAdmin { get; set; }
        public Country Country { get; set; }
        public List<Country> Countries { get; set; } = new List<Country>();
        public List<ViewContact> Contacts { get; set; } = new List<ViewContact>();

        protected override async Task OnInitializedAsync()
        {
            var httpClient = httpClientFactory.CreateClient(Configuration["Service:Api:AnonymousApiName"]);

            authenticateState = await authenticationStateTask;

            if (authenticateState.User?.Identity.IsAuthenticated == true)
            {
                await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
                {
                    Category = PageCategory,
                    Key = PageName,
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    UserId = authenticateState.User.FindFirstValue(ClaimTypes.NameIdentifier)
                });

                IsAdmin = (authenticateState.User.Identity.IsAuthenticated && authenticateState.User.HasClaim(ClaimTypes.Role, "admin"));
            }
            else
            {
                await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
                {
                    Category = PageCategory,
                    Key = PageName,
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid()
                });
            }

            var getCountries = await httpClient.GetAsync("api/contacts/countries");
            getCountries.EnsureSuccessStatusCode();

            var jCountries = await getCountries.Content.ReadAsStringAsync();
            Countries = JsonConvert.DeserializeObject<List<Country>>(jCountries);
        }

        async Task SelectCountry(Country country)
        {
            Country = country;
            var httpClient = httpClientFactory.CreateClient(Configuration["Service:Api:AnonymousApiName"]);

            authenticateState = await authenticationStateTask;

            if (authenticateState.User?.Identity.IsAuthenticated == true)
            {
                await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
                {
                    Category = PageCategory,
                    Key = $"GetContactByCountry_{country.Eeacode}",
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    UserId = authenticateState.User.FindFirstValue(ClaimTypes.NameIdentifier)
                });

                IsAdmin = (authenticateState.User.Identity.IsAuthenticated && authenticateState.User.HasClaim(ClaimTypes.Role, "admin"));
            }
            else
            {
                await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
                {
                    Category = PageCategory,
                    Key = $"GetContactByCountry_{country.Eeacode}",
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid()
                });
            }


            var getContacts = await httpClient.GetAsync($"api/contacts/bycountry/{country.Eeacode}");
            getContacts.EnsureSuccessStatusCode();

            var jContacts = await getContacts.Content.ReadAsStringAsync();
            Contacts = JsonConvert.DeserializeObject<List<ViewContact>>(jContacts);
        }

        void ClearCountrySelection()
        {
            Contacts = new List<ViewContact>();
            Country = null;
            StateHasChanged();
        }
    }
}