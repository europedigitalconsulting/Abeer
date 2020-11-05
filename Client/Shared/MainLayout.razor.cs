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
    public partial class MainLayout: LayoutComponentBase
    {
        public DateTime LastLogin { get; set; }
        protected ClaimsPrincipal _user;
        protected IEnumerable<Claim> _claims;
        protected string Name;
        protected string UserId;
        protected bool IsAuthenticated = false;
        protected string DisplayName;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        [Inject] private NavigationManager navigationManager { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await GetClaimsPrincipalData();
        }

        private async Task GetClaimsPrincipalData()
        {
            var authenticateSate = await authenticationStateTask;
            _user = authenticateSate.User;

            if (_user.Identity.IsAuthenticated)
            {
                _claims = _user.Claims;
                Name = _user.Identity.Name;
                IsAuthenticated = true;
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

    }
}
