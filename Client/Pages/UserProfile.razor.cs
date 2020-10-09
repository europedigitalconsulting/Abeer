using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class UserProfile : ComponentBase
    {
        [Inject]private NavigationManager NavigationManager { get; set; }
        [Inject]private HttpClient HttpClient { get; set; }

        public ApplicationUser User { get; set; } = new ApplicationUser();
        public bool ModalQrCodeVisible { get; set; }

        public bool ModalSocialNetworkVisible { get; set; }
        public string ProfileUrl => NavigationManager.ToAbsoluteUri($"/importContact/{User.Id}").ToString();
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync("api/Profile");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"user :{json}");
            User = JsonConvert.DeserializeObject<ApplicationUser>(json);
        }

        async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync("/api/Profile", User);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            User = JsonConvert.DeserializeObject<ApplicationUser>(json);
        }

        async Task ToggleModalSocialNetwork()
        {
            ModalSocialNetworkVisible = !ModalSocialNetworkVisible;
        }
    }
}
