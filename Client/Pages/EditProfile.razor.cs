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
        SocialNetwork NewSocialLink = new SocialNetwork();
        CustomLink NewCustomLink = new CustomLink();

        [Inject]private NavigationManager NavigationManager { get; set; }
        [Inject]private HttpClient HttpClient { get; set; }

        public ViewApplicationUser User { get; set; } = new ViewApplicationUser();
        
        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();

        public List<SocialNetwork> AvailableSocialNetworksToAdd { get; set; } = new List<SocialNetwork>();
        public List<SocialNetwork> SocialNetworkConnected { get; set; } = new List<SocialNetwork>();
        public List<CustomLink> CustomLinks { get; set; } = new List<CustomLink>();

        bool ModalSocialNetworkVisible;
        bool ModalCustomLinkVisible;
        bool ModalChangePassword;
        bool ChangePasswordHasError;
        string ChangePasswordError = "";

        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public string ProfileUrl => NavigationManager.ToAbsoluteUri($"/viewProfile/{User.Id}").ToString();
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync($"api/Profile");
            response.EnsureSuccessStatusCode();
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

        async Task ChangePassword()
        {
            if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) ||
                string.IsNullOrEmpty(ConfirmPassword))
            {

                ChangePasswordHasError = true;
                ChangePasswordError = Loc["PasswordFieldEmptyError"].Value;
            }
            if(NewPassword != ConfirmPassword)
            {
                ChangePasswordHasError = true;
                ChangePasswordError = Loc["PasswordNotConfirmedError"].Value;
            }
            else
            {
                ChangePasswordHasError = false;
                var response = await HttpClient.PutAsJsonAsync($"/api/Profile/ChangePassword", new ChangePasswordViewModel
                {
                    UserId = User.Id, OldPassword = OldPassword, NewPassword = NewPassword
                });
                ChangePasswordHasError = !response.IsSuccessStatusCode;
                Console.Write(response.Content.ReadAsStringAsync());
                ChangePasswordError = Loc["ChangePasswordFailedError"];

                if (!ChangePasswordHasError)
                    await ToogleChangePassword();
            }
        }
        async Task ToggleModalSocialNetwork()
        {
            ModalSocialNetworkVisible = !ModalSocialNetworkVisible;
        }

        async Task ToggleModalCustomLink()
        {
            ModalCustomLinkVisible = !ModalCustomLinkVisible;
        }

        async Task ToogleChangePassword()
        {
            ModalChangePassword = !ModalChangePassword;
        }

        async Task DeleteSocialNetwork(SocialNetwork socialNetwork)
        {
            var response = await HttpClient.DeleteAsync($"/api/SocialNetwork/{User.Id}/{socialNetwork.Name}");
            response.EnsureSuccessStatusCode();
            SocialNetworkConnected.Remove(socialNetwork);
            await InvokeAsync(StateHasChanged);
        }

        async Task DeleteCustomLink(CustomLink customLink)
        {
            var response = await HttpClient.DeleteAsync($"/api/CustomLink/{User.Id}/{customLink.Id}");
            response.EnsureSuccessStatusCode();
            CustomLinks.Remove(customLink);
            await InvokeAsync(StateHasChanged);
        }

        async Task OpenModalSocialNetwork()
        {
            NewSocialLink = new SocialNetwork { OwnerId = User.Id };
            await ToggleModalSocialNetwork();
        }

        async Task OpenModalCustomLink()
        {
            NewCustomLink = new CustomLink { OwnerId = User.Id };
            await ToggleModalCustomLink();
        }

        async Task AddSocialNetwork()
        {
            var response = await HttpClient.PostAsJsonAsync<SocialNetwork>($"/api/SocialNetwork", NewSocialLink);
            response.EnsureSuccessStatusCode();
            SocialNetworkConnected.Add(NewSocialLink);
            NewSocialLink = new SocialNetwork { OwnerId = User.Id };
            await InvokeAsync(StateHasChanged);
            await ToggleModalSocialNetwork();
        }

        async Task AddCustomLink()
        {
            var response = await HttpClient.PostAsJsonAsync<CustomLink>($"/api/CustomLink", NewCustomLink);
            response.EnsureSuccessStatusCode();
            CustomLinks.Add(NewCustomLink);
            NewCustomLink = new CustomLink { OwnerId = User.Id };
            await InvokeAsync(StateHasChanged);
            await ToggleModalCustomLink();
        }

        async Task SetSocialNetwork(string name, string background, string logo)
        {
            NewSocialLink.Name = name;
            NewSocialLink.BackgroundColor = background;
            NewSocialLink.Logo = logo;
            await InvokeAsync(StateHasChanged);
        }
    }
}