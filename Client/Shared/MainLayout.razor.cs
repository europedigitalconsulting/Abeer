using Abeer.Shared.ClientHub;
using Abeer.Shared.EventNotification;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Client.Shared
{
    public partial class MainLayout : LayoutComponentBase
    {
        private NotificationClient NotificationClient { get; set; }  
        public DateTime LastLogin { get; set; }
        protected ClaimsPrincipal _user;
        protected IEnumerable<Claim> _claims;
        protected string Name;
        protected string UserId;
        protected bool IsAuthenticated = false;
        protected string DisplayName;
        protected bool IsAdmin;
        public ScreenSize ScreenSize { get; set; } = new ScreenSize();
        [Inject] public HttpClient httpClient { get; set; }
        [CascadingParameter]  private Task<AuthenticationState> authenticationStateTask { get; set; }
        private AuthenticationState authenticationState { get; set; }
        [Inject] private NavigationManager navigationManager { get; set; }
        [Inject] IAccessTokenProvider tokenProvider { get; set; }
        public bool ModalOpenMessage { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;
            _user = authState.User;

            if (_user.Identity.IsAuthenticated)
            {
                Console.WriteLine($"start get notification for user {_user.Identity.Name}");

                var getNotifications = await httpClient.GetAsync("api/Notification");
                getNotifications.EnsureSuccessStatusCode();

                var json = await getNotifications.Content.ReadAsStringAsync();
                var accessTokenResult = await tokenProvider.RequestAccessToken();
                accessTokenResult.TryGetToken(out var accessToken); 
                NotificationClient = new NotificationClient(navigationManager.ToAbsoluteUri("/notification").AbsoluteUri, accessToken.Value);
                
                await NotificationClient.StartAsync();
                NotificationClient.NotificationEvent += ShowNotification; 
                if (!string.IsNullOrEmpty(json))
                {
                    var temp = JsonConvert.DeserializeObject<List<Notification>>(json); 
                    await NotificationClient.SendNotifications(temp);
                }
            }
        }
        public async void ShowNotification(object sender, NotificationEventArgs e)
        {
            NotificationClient.Notifications.Add(e.Notification);
            await InvokeAsync(StateHasChanged);
        }
        protected override async Task OnParametersSetAsync()
        {
            await GetClaimsPrincipalData();
        }

        private async Task GetClaimsPrincipalData()
        {
            authenticationState = await authenticationStateTask;
            _user = authenticationState.User;

            if (_user.Identity.IsAuthenticated)
            {
                _claims = _user.Claims;
                Name = _user.Identity.Name;
                IsAuthenticated = true;

                IsAdmin = authenticationState.User.HasClaim(ClaimTypes.Role, "admin");
                
                if (string.IsNullOrWhiteSpace(Name))
                    Name = UserId;

                DisplayName = _user.FindFirstValue("displayname");

                if (string.IsNullOrWhiteSpace(DisplayName))
                    DisplayName = Name;
            }
        }

        protected async Task BeginSignOut(MouseEventArgs args)
        {
            navigationManager.NavigateTo("authentication/logout");
        }

        protected async Task OpenMessageClicked()
        { 
            ModalOpenMessage = !ModalOpenMessage;
            StateHasChanged();
        }
    }
}
