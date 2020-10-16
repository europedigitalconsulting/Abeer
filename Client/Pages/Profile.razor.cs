using System;
using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Abeer.Client.Pages
{
    public partial class Profile : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient httpClient { get; set; }

        public ViewApplicationUser UserProfile { get; set; } = new ViewApplicationUser();

        protected override async Task OnInitializedAsync()
        {
            var response = await httpClient.GetAsync("api/Profile");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            UserProfile = JsonConvert.DeserializeObject<ViewApplicationUser>(json);
            Console.Write(json);
            NavigationUrlService.SetUrls($"https://www.google.com/maps/search/?api=1&query={UserProfile.Address},{UserProfile.City}%20{UserProfile.Country}&query_place_id={UserProfile.DisplayName}",
                null);
        }
    }
}