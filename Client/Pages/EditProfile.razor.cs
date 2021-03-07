using Abeer.Shared;
using Abeer.Shared.Functional;
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
        [Parameter]
        public EventCallback<ViewApplicationUser> CloseToggle { get; set; } 
        private CustomLink NewCustomLink = new CustomLink();

        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }
        [Parameter]
        public ViewApplicationUser User { get; set; } 

        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();

        public List<SocialNetwork> AvailableSocialNetworksToAdd { get; set; } = new List<SocialNetwork>();
        public List<SocialNetwork> SocialNetworkConnected { get; set; } = new List<SocialNetwork>();
        public List<CustomLink> CustomLinks { get; set; } = new List<CustomLink>();
        public bool IsReadOnly => User?.IsReadOnly ?? false;

        [Inject] private NavigationManager navigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync($"api/Profile");
            if (response.IsSuccessStatusCode)
            {   
                CustomLinks = User.CustomLinks.ToList();

                var responseSocialNetwork = await HttpClient.GetAsync("api/socialnetwork");
                response.EnsureSuccessStatusCode(); 
            }
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
            await HttpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
            {
                Category = "Navigation",
                Key = "EditProfile",
                CreatedDate = DateTime.UtcNow,
                Id = Guid.NewGuid(), 
                UserId = User.Id
            });

            var response = await HttpClient.PutAsJsonAsync("/api/Profile", User);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            User = JsonConvert.DeserializeObject<ViewApplicationUser>(json);


            await Close();
        }
        async Task Close()
        {
            await CloseToggle.InvokeAsync(User);
        }
    }
}