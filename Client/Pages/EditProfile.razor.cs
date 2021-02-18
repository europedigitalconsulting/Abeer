using Abeer.Shared;
using Abeer.Shared.ViewModels;

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
    public partial class EditProfile : ComponentBase
    {
        private SocialNetwork NewSocialLink = new SocialNetwork();
        private CustomLink NewCustomLink = new CustomLink();

        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }

        public ViewApplicationUser User { get; set; } = new ViewApplicationUser();

        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();

        public List<SocialNetwork> AvailableSocialNetworksToAdd { get; set; } = new List<SocialNetwork>();
        public List<SocialNetwork> SocialNetworkConnected { get; set; } = new List<SocialNetwork>();
        public List<CustomLink> CustomLinks { get; set; } = new List<CustomLink>();

        [Inject] private NavigationManager navigationManager { get; set; }  
         
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync($"api/Profile");
            if (response.IsSuccessStatusCode)
            { 
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"user :{json}");
                User = JsonConvert.DeserializeObject<ViewApplicationUser>(json); 
                NewSocialLink = new SocialNetwork { OwnerId = User.Id };

                SocialNetworkConnected = User.SocialNetworkConnected.ToList();
                CustomLinks = User.CustomLinks.ToList();

                var responseSocialNetwork = await HttpClient.GetAsync("api/socialnetwork");
                response.EnsureSuccessStatusCode();

                var jsonSocialNetwork = await responseSocialNetwork.Content.ReadAsStringAsync();
                AvailableSocialNetworks = JsonConvert.DeserializeObject<List<SocialNetwork>>(jsonSocialNetwork);

                AvailableSocialNetworks.ForEach(a =>
                {
                    if (!User.SocialNetworkConnected.ToList().Exists(c => a.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        AvailableSocialNetworksToAdd.Add(a);
                    }
                });
            }          
        } 
        private async Task DeleteSocialNetwork(SocialNetwork socialNetwork)
        {
            var response = await HttpClient.DeleteAsync($"/api/SocialNetwork/{User.Id}/{socialNetwork.Name}");
            response.EnsureSuccessStatusCode();
            SocialNetworkConnected.Remove(socialNetwork);
            await InvokeAsync(StateHasChanged);
        }

        private async Task DeleteCustomLink(CustomLink customLink)
        {
            var response = await HttpClient.DeleteAsync($"/api/CustomLink/{User.Id}/{customLink.Id}");
            response.EnsureSuccessStatusCode();
            CustomLinks.Remove(customLink);
            await InvokeAsync(StateHasChanged);
        }
        async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync("/api/Profile", User);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            User = JsonConvert.DeserializeObject<ViewApplicationUser>(json);
        }
    }
}