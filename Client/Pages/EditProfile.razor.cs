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
        private bool ModalSocialNetworkVisible;
        private bool ModalChangeMailVisible;
        private bool ModalCustomLinkVisible;
        private bool ModalChangePassword;
        private bool ModalDisplayPinCode;

        private bool ChangePasswordHasError;
        private bool ChangePhotoHasError;
        private bool ChangeChangeMailHasError;
        private bool ModalChangePhoto;
        private string ChangePasswordError = "";
        private string ChangeChangeMaildError = "";
        private string ChangePhotoError = "";
        private string _PhotoType = "Gravatar";
        private string DigitCode;
        private int PinCode;
        private string NewDigitCode;
        private int NewPinCode;

        public string PhotoType
        {
            get => _PhotoType;
            set
            {
                _PhotoType = value;
            }
        }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string NewMail { get; set; }
        public string ConfirmMail { get; set; }

        private string _PhotoUrl;
        private string Error;

        public string PhotoUrl
        {
            get => _PhotoUrl;
            set
            {
                _PhotoUrl = value;
            }
        }
        public string ProfileUrl => NavigationManager.ToAbsoluteUri($"/viewProfile/{User.Id}").ToString();
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync($"api/Profile");
            if (response.IsSuccessStatusCode)
            { 
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"user :{json}");
                User = JsonConvert.DeserializeObject<ViewApplicationUser>(json);

                DigitCode = User.DigitCode;
                PinCode = User.PinCode;

                _PhotoUrl = User.PhotoUrl;

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

        private async Task SaveNewCard()
        {
            ApplicationUser user = new ApplicationUser();

            user.PinDigit =  NewDigitCode ;
            user.PinCode = NewPinCode;

            var response = await HttpClient.PostAsJsonAsync($"api/Profile/SaveNewCard", user);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(); 
                var result = JsonConvert.DeserializeObject<ApplicationUser>(json);

                PinCode = result.PinCode;
                DigitCode = result.PinDigit.ToString();
                NewDigitCode = "";
                NewPinCode = 0;
            }
            else
            {
                Error = await response.Content.ReadAsStringAsync(); 
            }
            StateHasChanged();
        }

        private async Task ChangePhoto()
        {
            User.PhotoUrl = PhotoUrl;
            await Update();
            await ToggleChangePhoto();
        }

        private async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync("/api/Profile", User);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            User = JsonConvert.DeserializeObject<ViewApplicationUser>(json);
        }

        private async Task ChangePassword()
        {
            if (NewPassword != ConfirmPassword)
            {
                ChangePasswordHasError = true;
                ChangePasswordError = Loc["PasswordNotConfirmedError"].Value;
            }
            else
            {
                ChangePasswordHasError = false;
                var response = await HttpClient.PutAsJsonAsync($"/api/Profile/ChangePassword", new ChangePasswordViewModel
                {
                    UserId = User.Id,
                    OldPassword = OldPassword,
                    NewPassword = NewPassword
                });
                ChangePasswordHasError = !response.IsSuccessStatusCode;
                ChangePasswordError = Loc["ChangePasswordFailedError"];

                if (!ChangePasswordHasError)
                    await ToggleChangePassword();
            }
        }

        private async Task ToggleModalSocialNetwork()
        {
            ModalSocialNetworkVisible = !ModalSocialNetworkVisible;
        }

        private async Task ToggleModalChangeMail()
        {
            ModalChangeMailVisible = !ModalChangeMailVisible;
        }

        private async Task ToggleModalCustomLink()
        {
            ModalCustomLinkVisible = !ModalCustomLinkVisible;
        }

        private async Task ToggleChangePassword()
        {
            ModalChangePassword = !ModalChangePassword;
        }

        private async Task ToggleChangePhoto()
        {
            ModalChangePhoto = !ModalChangePhoto;
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

        private async Task OpenModalSocialNetwork()
        {
            NewSocialLink = new SocialNetwork { OwnerId = User.Id };
            await ToggleModalSocialNetwork();
        }

        private async Task OpenModalCustomLink()
        {
            NewCustomLink = new CustomLink { OwnerId = User.Id };
            await ToggleModalCustomLink();
        }

        private async Task OpenModalChangeMail()
        {
            NewCustomLink = new CustomLink { OwnerId = User.Id };
            await ToggleModalChangeMail();
        }

        private async Task ChangeMail()
        {
            if (string.IsNullOrEmpty(NewMail) || NewMail != ConfirmMail)
            {
                ChangeChangeMailHasError = true;
                ChangeChangeMaildError = Loc["PasswordNotConfirmedError"].Value;
            }
            else
            {
                ChangeChangeMailHasError = false;
                var response = await HttpClient.PutAsJsonAsync($"/api/Profile/ChangeEmail", new ChangeMailViewModel
                {
                    UserId = User.Id,
                    OldMail= User.Email,
                    NewMail = NewMail
                });
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    ChangeChangeMailHasError = true;
                    ChangeChangeMaildError = Loc["MailAlreadyExist"].Value;
                }
                else if (response.IsSuccessStatusCode)
                {
                    navigationManager.NavigateTo(navigationManager.ToAbsoluteUri("Identity/account/logout?returnUrl=/Profile").ToString(), true);
                } 
            } 
        }

        private async Task AddSocialNetwork()
        {
            var response = await HttpClient.PostAsJsonAsync<SocialNetwork>($"/api/SocialNetwork", NewSocialLink);
            response.EnsureSuccessStatusCode();
            SocialNetworkConnected.Add(NewSocialLink);
            NewSocialLink = new SocialNetwork { OwnerId = User.Id };
            await InvokeAsync(StateHasChanged);
            await ToggleModalSocialNetwork();
        }

        private async Task AddCustomLink()
        {
            var response = await HttpClient.PostAsJsonAsync<CustomLink>($"/api/CustomLink", NewCustomLink);
            response.EnsureSuccessStatusCode();
            CustomLinks.Add(NewCustomLink);
            NewCustomLink = new CustomLink { OwnerId = User.Id };
            await InvokeAsync(StateHasChanged);
            await ToggleModalCustomLink();
        }

        private async Task SetSocialNetwork(string name, string background, string logo)
        {
            NewSocialLink.Name = name;
            NewSocialLink.BackgroundColor = background;
            NewSocialLink.Logo = logo;
            await InvokeAsync(StateHasChanged);
        }

        private async Task OpenModalPinCode()
        {
            ModalDisplayPinCode = true;
            await InvokeAsync(StateHasChanged);
        }
    }
}