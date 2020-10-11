using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class UserProfile : ComponentBase
    {
        [Inject]private NavigationManager NavigationManager { get; set; }
        [Inject]private HttpClient HttpClient { get; set; }

        public ViewApplicationUser User { get; set; } = new ViewApplicationUser();
        
        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();

        public List<SocialNetwork> AvailableSocialNetworksToAdd { get; set; } = new List<SocialNetwork>();
        public bool ModalQrCodeVisible { get; set; }

        public bool ModalSocialNetworkVisible { get; set; }
        public string ProfileUrl => NavigationManager.ToAbsoluteUri($"/importContact/{User.Id}").ToString();
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync("api/Profile");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"user :{json}");
            User = JsonConvert.DeserializeObject<ViewApplicationUser>(json);

            var responseSocialNetwork = await HttpClient.GetAsync("api/socialnetwork");
            response.EnsureSuccessStatusCode();
            var jsonSocialNetwork = await responseSocialNetwork.Content.ReadAsStringAsync();
            AvailableSocialNetworks = JsonConvert.DeserializeObject<List<SocialNetwork>>(jsonSocialNetwork);

            AvailableSocialNetworks.ForEach(a =>
            {
                if(!User.SocialNetworkConnected.ToList().Exists(c => a.Name.Equals(c.Name,StringComparison.OrdinalIgnoreCase)))
                {
                    AvailableSocialNetworksToAdd.Add(a);
                }
            });
        }

        async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync("/api/Profile", User);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            User = JsonConvert.DeserializeObject<ViewApplicationUser>(json);
        }

        async Task ToggleModalSocialNetwork()
        {
            ModalSocialNetworkVisible = !ModalSocialNetworkVisible;
        }
    }
}
