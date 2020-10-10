using System;
using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class Profile : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient httpClient { get; set; }

        public string ProfileUrl => NavigationManager.ToAbsoluteUri($"/profile/{User.Id}").ToString();
        public ApplicationUser User { get; set; } = new ApplicationUser();

        protected override async Task OnInitializedAsync()
        {
            var response = await httpClient.GetAsync("api/Profile");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"user :{json}");
            User = JsonConvert.DeserializeObject<ApplicationUser>(json);
        }
    }
}