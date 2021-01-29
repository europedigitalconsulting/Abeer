using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web; 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Client.Shared
{
    public partial class MainLayout : LayoutComponentBase
    {
        public DateTime LastLogin { get; set; }
        protected ClaimsPrincipal _user;
        protected IEnumerable<Claim> _claims;
        protected string Name;
        protected string UserId;
        protected bool IsAuthenticated = false;
        protected string DisplayName;
        protected DateTime? SubscriptionEnd;
        protected bool IsUnlimited;
        protected bool IsAdmin;
        public ScreenSize ScreenSize { get; set; } = new ScreenSize();

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        private AuthenticationState authenticationState { get; set; }
        [Inject] private NavigationManager navigationManager { get; set; }

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
                if (!string.IsNullOrWhiteSpace(_user.FindFirstValue("subscribeEnd")))
                    SubscriptionEnd = DateTime.Parse(_user.FindFirstValue("subscribeEnd"));
                if (!string.IsNullOrWhiteSpace(_user.FindFirstValue("IsUnlimited")))
                    IsUnlimited = bool.Parse(_user.FindFirstValue("IsUnlimited"));
            }
        }

        protected async Task BeginSignOut(MouseEventArgs args)
        {
            navigationManager.NavigateTo("authentication/logout");
        }

    }
}
