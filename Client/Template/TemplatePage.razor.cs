using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.ClientHub;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Abeer.Client.Template
{
    public partial class TemplatePage : ComponentBase
    {
        private const string PageName = "TemplatePage";
        private const string PageCategory = "Template";

        [Inject] private IHttpClientFactory httpClientFactory { get; set; }
        [CascadingParameter]
        public ScreenSize ScreenSize { get; set; }
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        private AuthenticationState authenticateState { get; set; }
        private bool IsAdmin { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var httpClient = httpClientFactory.CreateClient(Configuration["Service:Api:AnonymousApiName"]);

            authenticateState = await authenticationStateTask;

            if (authenticateState.User?.Identity.IsAuthenticated == true)
            {
                await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
                {
                    Category = PageCategory,
                    Key = PageName,
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    UserId = authenticateState.User.FindFirstValue(ClaimTypes.NameIdentifier)
                });

                IsAdmin = (authenticateState.User.Identity.IsAuthenticated && authenticateState.User.HasClaim(ClaimTypes.Role, "admin"));
            }
            else
            {
                await httpClient.PostAsJsonAsync<EventTrackingItem>("api/EventTracker", new EventTrackingItem
                {
                    Category = PageCategory,
                    Key = PageName,
                    CreatedDate = DateTime.UtcNow,
                    Id = Guid.NewGuid()
                });
            }
        }
    }
}
