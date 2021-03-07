using Abeer.Client.Shared.NotificationDialogs;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Client.Shared
{
    public partial class NotificationCtrl : ComponentBase
    {
        private List<Notification> Notifications { get; set; }
        private HubConnection HubConnection { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public HttpClient httpClient { get; set; }
        [CascadingParameter] public Task<AuthenticationState> authenticationStateTask { get; set; }
        public ClaimsPrincipal User { get; set; }

        Notification _next;

        public Dictionary<string, Type> DialogTypes = new()
        {
            { "welcome", typeof(WelcomeDialog) },
            { "daily-reminder", typeof(DailyReminderDialog) },
            { "expiredprofile", typeof(ExpiredProfileDialog) },
            { "soonexpireprofile", typeof(SoonExpiredProfileDialog) }
        };

        public Notification NextNotification
        {
            get
            {
                if (_next == null)
                    _next = Notifications?.FirstOrDefault();

                return _next;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;
            User = authState.User;

            if (User.Identity.IsAuthenticated)
            {
                Console.WriteLine($"start get notification for user {User.Identity.Name}");

                var getNotifications = await httpClient.GetAsync("api/Notification");
                getNotifications.EnsureSuccessStatusCode();

                var json = await getNotifications.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(json))
                {
                    Notifications = JsonConvert.DeserializeObject<List<Notification>>(json);
                }

                Console.WriteLine($"getted notifications {Notifications.Count}");

                HubConnection = new HubConnectionBuilder().WithAutomaticReconnect()
                    .WithUrl(NavigationManager.ToAbsoluteUri("/notification")).Build();

                HubConnection.On<Notification>("OnNotification", (notification) =>
                {
                    Notifications.Add(notification);
                    StateHasChanged();
                });

                await HubConnection.StartAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await HubConnection.DisposeAsync();
        }

        public async Task SetDisplayedNotification()
        {
            _next.IsDisplayed = true;
            _next.DisplayCount += 1;
            _next.LastDisplayTime = DateTime.UtcNow;
            var post = await httpClient.PutAsJsonAsync<Notification>("api/notification", _next);
            post.EnsureSuccessStatusCode();
            Notifications.Remove(_next);
            _next = null;
        }

        public RenderFragment RenderDialog(string type) => builder =>
        {
            builder.OpenComponent(0, DialogTypes[type]);
            builder.AddAttribute(1, "User", User);
            builder.AddAttribute(2, "Close", EventCallback.Factory.Create(this, () => SetDisplayedNotification()));
            builder.CloseComponent();
        };
    }
}
