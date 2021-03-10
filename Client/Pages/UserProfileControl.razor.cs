using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Abeer.Client.Pages
{
    public partial class UserProfileControl : ComponentBase
    {
        [Parameter]
        public bool ReadOnly { get; set; }
        [Parameter]
        public string ProfileUrl { get; set; }
        [Parameter]
        public ViewApplicationUser User { get; set; } = new ViewApplicationUser();
        [Parameter]
        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();

        [CascadingParameter]
        public ScreenSize ScreenSize { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }

        public List<SocialNetwork> AvailableSocialNetworksToAdd { get; set; } = new List<SocialNetwork>();


        private bool OpenList { get; set; }
        private bool TabMap { get; set; }
        private bool ModalQrCode { get; set; }
        private bool ToggleMenu { get; set; }
        private bool ModalEditProfil;
        private bool ModalChangeMail;
        private bool ModalChangePassword;
        private bool ModalChangePinCode;
        private bool ModalSocialNetwork;
        private bool ModalCustomLink;
        private string DigitCode;
        private int PinCode;
        private string NewDigitCode;
        private int NewPinCode;

        private string ChangePhotoError = "";
        private string _PhotoType = "Gravatar";
        private bool ModalChangePhoto;

        private bool ChangePhotoHasError;
        private bool ChangePasswordHasError;
        private bool ChangeChangeMailHasError;
        private string ChangeChangeMaildError = "";
        private string ChangePasswordError = "";
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string NewMail { get; set; }
        public string ConfirmMail { get; set; }
        private string Error;
        public string PhotoType
        {
            get => _PhotoType;
            set
            {
                _PhotoType = value;
            }
        } 

        protected override async Task OnInitializedAsync()
        {
            AvailableSocialNetworks.ForEach(a =>
            {
                if (!User.SocialNetworkConnected.ToList().Exists(c => a.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    AvailableSocialNetworksToAdd.Add(a); 
                }
            });
            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (string.IsNullOrEmpty(User.PhotoUrl))
            {
                User.PhotoUrl = "https://www.gravatar.com/avatar.php?gravatar_id=e511eeb916b3fa2202f38abfa29532b0&amp;rating=PG&amp;size=1600";
            }

            await InvokeAsync(StateHasChanged);
        }
     

        private SocialNetwork NewSocialLink = new SocialNetwork();
        private CustomLink NewCustomLink = new CustomLink();

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
                    ModalChangePassword = false;
            }
        }
        private async Task SaveNewCard()
        {
            ApplicationUser user = new ApplicationUser();

            user.PinDigit = NewDigitCode;
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
            User.PhotoUrl = User.PhotoUrl;
            var response = await HttpClient.PutAsJsonAsync("/api/Profile", User);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            User = JsonConvert.DeserializeObject<ViewApplicationUser>(json);
            ModalChangePhoto = false;
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
                    OldMail = User.Email,
                    NewMail = NewMail
                });
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    ChangeChangeMailHasError = true;
                    ChangeChangeMaildError = Loc["MailAlreadyExist"].Value;
                }
                else if (response.IsSuccessStatusCode)
                {
                    NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri("Identity/account/logout?returnUrl=/Profile").ToString(), true);
                }
            }
        } 
        private void OpenModalChangePassword()
        {
            ModalChangePassword = true;
            ToggleMenu = false;
        }
        private void OpenModalChangePinCode()
        {
            ModalChangePinCode = true;
            ToggleMenu = false;
        }
        private void OpenModalChangePhoto()
        {
            ModalChangePhoto = true;
            ToggleMenu = false;
        }
        private void OpenModalChangeMail()
        {
            ModalChangeMail = true;
            ToggleMenu = false;
        }
        private void OpenModalEditProfil()
        {
            ModalEditProfil = true;
            ToggleMenu = false;
        }
        private void OpenModalSocialNetwork()
        {
            NewSocialLink = new SocialNetwork { OwnerId = User.Id };
            ModalSocialNetwork = true;
            OpenList = false;
            ToggleMenu = false;
        }
        private void OpenModalCustomLink()
        {
            NewCustomLink = new CustomLink { OwnerId = User.Id };
            ModalCustomLink = true;
            ToggleMenu = false;
        }
        private async Task AddSocialNetwork()
        {
            if (!string.IsNullOrEmpty(NewSocialLink.Url) && !string.IsNullOrEmpty(NewSocialLink.Name))
            {
                var response = await HttpClient.PostAsJsonAsync<SocialNetwork>($"/api/SocialNetwork", NewSocialLink);
                response.EnsureSuccessStatusCode();
                User.SocialNetworkConnected.Add(NewSocialLink);
                NewSocialLink = new SocialNetwork { OwnerId = User.Id };
                await InvokeAsync(StateHasChanged); 
            }
        }
        private async Task AddCustomLink()
        {
            var response = await HttpClient.PostAsJsonAsync<CustomLink>($"/api/CustomLink", NewCustomLink);
            response.EnsureSuccessStatusCode();
            User.CustomLinks.Add(NewCustomLink);
            NewCustomLink = new CustomLink { OwnerId = User.Id };
            await InvokeAsync(StateHasChanged);
            ModalCustomLink = false;
        }
        private async Task SetSocialNetwork(string name, string background, string logo)
        {
            NewSocialLink.Name = name;
            NewSocialLink.BackgroundColor = background;
            NewSocialLink.Logo = logo;
            await InvokeAsync(StateHasChanged);
        }
        private async Task UpdateProfil(ViewApplicationUser user)
        {
            User = user;
            ModalEditProfil = false;
            StateHasChanged();
        } 
        private async Task DeleteSocialNetwork(SocialNetwork socialNetwork)
        {
            var response = await HttpClient.DeleteAsync($"/api/SocialNetwork/{User.Id}/{socialNetwork.Name}");
            response.EnsureSuccessStatusCode();
            AvailableSocialNetworks.Remove(socialNetwork);
            User.SocialNetworkConnected.Remove(socialNetwork);
            await InvokeAsync(StateHasChanged);
        }
    }
}
