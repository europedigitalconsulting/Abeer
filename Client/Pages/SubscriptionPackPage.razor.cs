using Abeer.Shared;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class SubscriptionPackPage : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        [Inject] private NavigationManager navigationManager { get; set; }
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] private HttpClient httpClient { get; set; }
        [Inject] private IConfiguration Configuration { get; set; }
        private List<SubscriptionPack> listSubscriptionPack { get; set; } = new List<SubscriptionPack>();
        public string CurrentCulture { get; set; }
        protected override async Task OnInitializedAsync()
        {
            CurrentCulture = CultureInfo.CurrentCulture.DisplayName;

            var authState = await authenticationStateTask;
            var user = authState.User;

            var httpClient = HttpClientFactory.CreateClient(Configuration["Service:Api:ApiName"]);

            await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
            {
                Category = "Subscription",
                Key = "start",
                CreatedDate = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)
            });

            var subscriptionPacks = await httpClient.GetAsync("api/SubPack/GetAll");

            if (subscriptionPacks.IsSuccessStatusCode)
            {
                var json = await subscriptionPacks.Content.ReadAsStringAsync();
                listSubscriptionPack = JsonConvert.DeserializeObject<List<SubscriptionPack>>(json);
            }

            await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
            {
                Category = "Subscription",
                Key = "Start",
                CreatedDate = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)
            });

            await base.OnInitializedAsync();
        }
        protected async Task Select(SubscriptionPack item)
        {
            if (item.Price == 0)
            {
                var subscriptionPack = await httpClient.PostAsJsonAsync<SubscriptionPack>("/api/SubPack/Select", item);
                if (subscriptionPack.IsSuccessStatusCode)
                {
                    navigationManager.NavigateTo("/", true);
                }
            }
            else
            {
                navigationManager.NavigateTo($"/payment-subscription/{item.Id}", true);
            }
        }
    }
}
