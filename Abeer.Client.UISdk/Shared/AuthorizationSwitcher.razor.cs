using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Abeer.Client.UISdk.Shared
{
    public partial class AuthorizationSwitcher : ComponentBase
    {
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Parameter]
        public RenderFragment Authorized { get; set; }
        [Parameter]
        public RenderFragment NotAuthorized { get; set; }
        [Parameter]
        public string Role { get; set; }
        [Parameter]
        public string Policy { get; set; }

        protected bool ShowLoader { get; set; }
        private RenderFragment CurrentView { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsAuthorized { get; set; }
        public ClaimsPrincipal User { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var authState = await authenticationStateTask;
            User = authState.User;

            IsAuthenticated = (User.Identity.IsAuthenticated);

            if (IsAuthenticated)
            {
                if(!string.IsNullOrWhiteSpace(Role))
                {
                    IsAuthorized = User.HasClaim(ClaimTypes.Role, Role);
                    Console.WriteLine($"Check User With role {Role} result {IsAuthorized}");
                }

                if (!string.IsNullOrWhiteSpace(Policy))
                {
                    var authorizationResult = await AuthorizationService.AuthorizeAsync(User, null, Policy);
                    IsAuthorized = authorizationResult.Succeeded;
                    Console.WriteLine($"Check User With Policy {Policy} result {IsAuthorized}");
                }

                if (string.IsNullOrWhiteSpace(Role) && string.IsNullOrWhiteSpace(Policy))
                    IsAuthorized = true;
            }
            else
            {
                IsAuthorized = false;
            }

            if (IsAuthorized)
                CurrentView = Authorized;
            else
                CurrentView = NotAuthorized;

            await InvokeAsync(StateHasChanged);
        }

    }
}
