using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MixERP.Net.VCards;
using MixERP.Net.VCards.Models;
using MixERP.Net.VCards.Serializer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
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
        public ViewApplicationUser Profile { get; set; } = new ViewApplicationUser();
        [Parameter]
        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();
        [Parameter]
        public bool IsAuthenticated { get; set; }
        [Parameter]
        public string ProfileId { get; set; }
        [CascadingParameter]
        public ScreenSize ScreenSize { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        public List<SocialNetwork> AvailableSocialNetworksToAdd { get; set; } = new List<SocialNetwork>();

        [Inject]
        IJSRuntime ThisJSRuntime { get; set; }

        private bool OpenList { get; set; }
        private bool TabBookmark { get; set; }
        private bool TabQrCode { get; set; }
        private bool ModalQrCode { get; set; }
        private bool ToggleMenu { get; set; }
        private bool ModalEditProfil;
        private bool ModalEditQrcode;
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
        public string KeyWord;
        private string Error;
        private bool DisplayModifyPinCode;
        private bool ModalOrganization;
        bool DisplayListOrganization;
        bool DisplayListTeam;
        bool DisplayListManager;
        private bool NotFoundTeam;
        private bool EditTeam;
        bool NotFoundOrganization;
        bool EditOrganization = true;
        bool EditManager = false;
        Organization Organization = null;
        Team Team = null;
        private bool IsNewOrganization;
        private bool IsNewTeam;
        ViewApplicationUser Manager;
        bool NotFoundManager;

        IList<Organization> ListOrganizations { get; set; } = new List<Organization>();
        ProfileOrganizationViewModel ProfileOrganization { get; set; }
        IList<Team> ListTeams { get; set; } = new List<Team>();
        IList<ViewApplicationUser> ListManagers { get; set; } = new List<ViewApplicationUser>();

        string SearchOrganization
        {
            get => searchOrganization; set
            {
                searchOrganization = value;

                if (!string.IsNullOrEmpty(searchOrganization) && searchOrganization.Length >= 3)
                {
                    Task.Run(async () => await StartSearchOrganization());
                }
            }
        }

        string SearchTeam
        {
            get => searchTeam; set
            {
                searchTeam = value;

                if (!string.IsNullOrEmpty(searchTeam) && searchTeam.Length >= 3)
                {
                    Task.Run(async () => await StartSearchTeam());
                }
            }
        }

        string SearchManager
        {
            get => searchManager; set
            {
                searchManager = value;

                if (!string.IsNullOrEmpty(searchManager) && searchManager.Length >= 5)
                {
                    Task.Run(async () => await StartSearchManager());
                }
            }
        }

        public string PhotoType
        {
            get => _PhotoType;
            set
            {
                _PhotoType = value;
            }
        }

        public string GetProfileMapQuery()
        {
            var list = new List<string>();

            if (!string.IsNullOrEmpty(Profile.Address))
                list.Add(Profile.Address.Replace(" ", "+"));

            if (!string.IsNullOrEmpty(Profile.City))
                list.Add($"+{Profile.City.Replace(" ", "+")}");

            if (!string.IsNullOrEmpty(Profile.Country))
                list.Add($"+{Profile.Country.Replace(" ", "+")}");

            return string.Join(",", list);
        }

        public Contact Link { get; set; }
        public ClaimsPrincipal CurrentUser { get; set; }
        public string CurrentUserId => CurrentUser?.FindFirstValue(ClaimTypes.NameIdentifier);

        protected override async Task OnInitializedAsync()
        {
            var authenticateState = await AuthenticationStateTask;

            CurrentUser = authenticateState.User;

            AvailableSocialNetworks.ForEach(a =>
            {
                if (!Profile.SocialNetworkConnected.ToList().Exists(c => a.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    AvailableSocialNetworksToAdd.Add(a);
                }
            });

            if (CurrentUser.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(Profile?.Id))
            {
                if (CurrentUserId != Profile.Id)
                {
                    var getLink = await HttpClient.GetAsync($"api/contacts/link/{Profile.Id}");

                    if (getLink.IsSuccessStatusCode)
                    {
                        var jLink = await getLink.Content.ReadAsStringAsync();
                        Link = JsonConvert.DeserializeObject<Contact>(jLink);
                    }
                }
            }

            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (string.IsNullOrEmpty(Profile.PhotoUrl))
            {
                Profile.PhotoUrl = "https://www.gravatar.com/avatar.php?gravatar_id=e511eeb916b3fa2202f38abfa29532b0&amp;rating=PG&amp;size=1600";
            }

            await InvokeAsync(StateHasChanged);
        }


        public async Task LinkContact()
        {
            var response = await HttpClient.GetAsync($"/api/contacts/{Profile.Id}");
            response.EnsureSuccessStatusCode();

            var getLink = await HttpClient.GetAsync($"api/contacts/link/{Profile.Id}");

            if (getLink.IsSuccessStatusCode)
            {
                var jLink = await getLink.Content.ReadAsStringAsync();
                Link = JsonConvert.DeserializeObject<Contact>(jLink);
            }
        }

        public void ToggleOpenList()
        {
            OpenList = !OpenList;
        }

        public async Task Unlink()
        {
            var response = await HttpClient.DeleteAsync($"/api/contacts/{Link.Id}");
            response.EnsureSuccessStatusCode();
            Link = null;
            await InvokeAsync(StateHasChanged);
        }

        private SocialNetwork NewSocialLink = new SocialNetwork();
        private CustomLink NewCustomLink = new CustomLink();
        private string searchOrganization;
        private string searchTeam;
        private string searchManager;

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
                    UserId = Profile.Id,
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
                DisplayModifyPinCode = false;
            }
            else
            {
                Error = await response.Content.ReadAsStringAsync();
            }
            StateHasChanged();
        }

        async Task DownloadVCard()
        {
            var vcard = new VCard
            {
                Version = MixERP.Net.VCards.Types.VCardVersion.V4,
                FormattedName = Profile.DisplayName,
                FirstName = Profile.FirstName,
                LastName = Profile.LastName,
                Classification = MixERP.Net.VCards.Types.ClassificationType.Confidential,
                Categories = new string[] { "meetag", "Friend", Profile.DisplayName, Profile.City, Profile.Country },
                Emails = new List<Email> {
                    new Email
                    {
                        EmailAddress = Profile.Email, Type = MixERP.Net.VCards.Types.EmailType.Smtp
                    }
                },
                Addresses = new List<Address>
                {
                    new Address
                    {
                        Country = Profile.Country, Locality = Profile.City, Street = Profile.Address
                    }
                },
                Telephones = new List<Telephone> {
                    new Telephone
                    {
                        Number = Profile.PhoneNumber, Type = MixERP.Net.VCards.Types.TelephoneType.Preferred
                    }
                },
                Url = NavigationManager.ToAbsoluteUri($"profile/viewprofile/{Profile.Id}"),
                LastRevision = DateTime.UtcNow,
                Title = Profile.Title,
                UniqueIdentifier = Profile.Id
            };

            var bytes = Encoding.UTF8.GetBytes(vcard.Serialize());

            await ThisJSRuntime.InvokeVoidAsync(
                "downloadFromByteArray",
                new
                {
                    ByteArray = Convert.ToBase64String(bytes),
                    FileName = $"vcard_{Profile.DisplayName.Replace(" ", "")}.vcf",
                    ContentType = "text/vcard"
                });
        }

        private async Task AddQrCode()
        {
            QrcodeViewModel model = new QrcodeViewModel();
            model.Key = KeyWord;
            model.OwnerId = Guid.Parse(CurrentUserId);

            var response = await HttpClient.PostAsJsonAsync($"api/Qrcode", model);

            if (response.IsSuccessStatusCode)
            {
                KeyWord = "";
            }
            StateHasChanged();
        }
        private async Task ChangePhoto()
        {
            Profile.PhotoUrl = Profile.PhotoUrl;
            var response = await HttpClient.PutAsJsonAsync("/api/Profile", Profile);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Profile = JsonConvert.DeserializeObject<ViewApplicationUser>(json);
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
                    UserId = Profile.Id,
                    OldMail = Profile.Email,
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
            PinCode = Profile.PinCode;
            DigitCode = Profile.DigitCode;

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
        private void OpenModalQrcode()
        {
            ModalEditQrcode = true;
            ToggleMenu = false;
        }


        private void OpenModalSocialNetwork()
        {
            NewSocialLink = new SocialNetwork { OwnerId = Profile.Id };
            ModalSocialNetwork = true;
            OpenList = false;
            ToggleMenu = false;
        }
        private void OpenModalCustomLink()
        {
            NewCustomLink = new CustomLink { OwnerId = Profile.Id };
            ModalCustomLink = true;
            ToggleMenu = false;
        }
        private async Task AddSocialNetwork()
        {
            if (!string.IsNullOrEmpty(NewSocialLink.Url) && !string.IsNullOrEmpty(NewSocialLink.Name))
            {
                var response = await HttpClient.PostAsJsonAsync<SocialNetwork>($"/api/SocialNetwork", NewSocialLink);
                response.EnsureSuccessStatusCode();
                Profile.SocialNetworkConnected.Add(NewSocialLink);
                NewSocialLink = new SocialNetwork { OwnerId = Profile.Id };
                await InvokeAsync(StateHasChanged);
            }
        }
        private async Task AddCustomLink()
        {
            var response = await HttpClient.PostAsJsonAsync<CustomLink>($"/api/CustomLink", NewCustomLink);
            response.EnsureSuccessStatusCode();
            Profile.CustomLinks.Add(NewCustomLink);
            NewCustomLink = new CustomLink { OwnerId = Profile.Id };
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
            Profile = user;
            ModalEditProfil = false;
            StateHasChanged();
        }
        private async Task DeleteSocialNetwork(SocialNetwork socialNetwork)
        {
            var response = await HttpClient.DeleteAsync($"/api/SocialNetwork/{Profile.Id}/{socialNetwork.Name}");
            response.EnsureSuccessStatusCode();
            AvailableSocialNetworks.Remove(socialNetwork);
            Profile.SocialNetworkConnected.Remove(socialNetwork);
            await InvokeAsync(StateHasChanged);
        }

        public async Task AcceptInvitation()
        {
            var response = await HttpClient.PutAsJsonAsync<Contact>($"/api/Contacts/Accept/{Link.Id}", Link);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Link = JsonConvert.DeserializeObject<Contact>(json);
            await InvokeAsync(StateHasChanged);
        }

        async Task SaveProfileOrganization()
        {
            var response = await HttpClient.PutAsJsonAsync<ProfileOrganizationViewModel>($"api/contacts/profileOrganization/{Profile.Id}", ProfileOrganization);
            response.EnsureSuccessStatusCode();
            ModalOrganization = false;
        }

        async Task DisplayOrganization()
        {
            var getOrganization = await HttpClient.GetAsync($"api/contacts/profileorganization/{Profile.Id}");
            getOrganization.EnsureSuccessStatusCode();
            var json = await getOrganization.Content.ReadAsStringAsync();
            ProfileOrganization = JsonConvert.DeserializeObject<ProfileOrganizationViewModel>(json);
            Organization = ProfileOrganization.Organization;
            Team = ProfileOrganization.Team;
            Manager = ProfileOrganization.Manager;
            SetOrganization(Organization);
            SetTeam(Team);
            SetManager(Manager);
            ModalOrganization = true;
        }

        async Task StartSearchOrganization()
        {
            var searchOrganizationResult = await HttpClient.GetAsync($"api/contacts/organization?searchTerm={SearchOrganization}");
            searchOrganizationResult.EnsureSuccessStatusCode();
            ListOrganizations = JsonConvert.DeserializeObject<List<Organization>>(await searchOrganizationResult.Content.ReadAsStringAsync());
            DisplayListOrganization = ListOrganizations?.Any() == true;
            NotFoundOrganization = !DisplayListOrganization;
        }

        async Task StartSearchTeam()
        {
            var searchTeamResult = await HttpClient.GetAsync($"api/contacts/team/{Organization.Id}?searchTerm={SearchTeam}");
            searchTeamResult.EnsureSuccessStatusCode();
            ListTeams = JsonConvert.DeserializeObject<List<Team>>(await searchTeamResult.Content.ReadAsStringAsync());
            DisplayListTeam = ListTeams?.Any() == true;
            NotFoundTeam = !DisplayListTeam;
        }

        async Task StartSearchManager()
        {
            var searchManagerResult = await HttpClient.GetAsync($"api/contacts/manager/{Organization.Id}/{Team.Id}?searchTerm={SearchManager}");
            searchManagerResult.EnsureSuccessStatusCode();
            ListManagers = JsonConvert.DeserializeObject<List<ViewApplicationUser>>(await searchManagerResult.Content.ReadAsStringAsync());
            DisplayListManager = ListManagers?.Any() == true;
            NotFoundManager = !DisplayListManager;
        }

        async Task CreateOrganization()
        {
            Organization = new()
            {
                CreatorId = Profile.Id,
                Description = SearchOrganization,
                Name = SearchOrganization,
                ManagerId = Profile.Id
            };

            var createOrganizationResult = await HttpClient.PostAsJsonAsync("api/contacts/organization", Organization);

            createOrganizationResult.EnsureSuccessStatusCode();

            IsNewOrganization = true;

            SetOrganization(Organization);

            NotFoundOrganization = false;

            await InvokeAsync(StateHasChanged);
        }

        async Task CreateTeam()
        {
            Team = new()
            {
                OrganizationId = Organization.Id,
                CreatorId = Profile.Id,
                Description = SearchTeam,
                Name = SearchTeam,
                ManagerId = Profile.Id
            };

            var createTeamResult = await HttpClient.PostAsJsonAsync($"api/contacts/team/{Organization.Id}", Team);

            createTeamResult.EnsureSuccessStatusCode();

            IsNewTeam = true;
            SetTeam(Team);

            await InvokeAsync(StateHasChanged);
        }

        private void SetTeam(Team team)
        {
            if (team == null)
                return;


            Team = team;
 
            DisplayListTeam = false;
            NotFoundTeam = false;
            EditTeam = false;
            EditManager = true;

            if (ProfileOrganization == null)
                ProfileOrganization = new ProfileOrganizationViewModel(new ProfileOrganization
                {
                    ContactId = Profile.Id,
                    TeamId = Team.Id,
                    OrganizationId = Organization.Id
                });
            else
                ProfileOrganization.SetTeam(Team);
        }

        void SetOrganization(Organization organization)
        {
            if (organization == null)
                return;

            Organization = organization;

            if (ProfileOrganization == null)
                ProfileOrganization = new ProfileOrganizationViewModel(new ProfileOrganization
                {
                    ContactId = Profile.Id,
                    OrganizationId = Organization.Id
                });
            else
                ProfileOrganization.SetOrganization(Organization);

            DisplayListOrganization = false;
            NotFoundOrganization = false;
            EditOrganization = false;
            EditTeam = true;
        }

        void StartEditOrganization()
        {
            EditOrganization = true;
            NotFoundOrganization = false;
            SearchOrganization = Organization.Name;
        }

        void StartEditTeam()
        {
            EditTeam = true;
            NotFoundTeam = false;
            SearchTeam = Team.Name;
        }

        void StartEditManager()
        {
            EditManager = true;
            NotFoundManager = false;
            SearchManager = Manager?.DisplayName;
        }

        void SetManager(ViewApplicationUser manager)
        {
            if (manager == null)
                return;

            Manager = manager;

            if (ProfileOrganization == null)
                ProfileOrganization = new ProfileOrganizationViewModel(new ProfileOrganization
                {
                    ContactId = Profile.Id,
                    OrganizationId = Organization.Id, 
                    ManagerId = Manager.Id
                });
            else
                ProfileOrganization.SetManager(Manager);

            DisplayListManager = false;
            NotFoundManager = false;
            EditManager = false;
        }
    }
}