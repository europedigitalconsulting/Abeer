using Abeer.Client.Shared.NotificationDialogs;
using Abeer.Shared.ClientHub;
using Abeer.Shared.EventNotification;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
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
        [Parameter] public NotificationClient NotificationClient { get; set; }
        [Parameter] public ClaimsPrincipal User { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public HttpClient httpClient { get; set; }
        [CascadingParameter] public Task<AuthenticationState> authenticationStateTask { get; set; }

        Notification _next;

        (TouchPoint ReferencePoint, DateTime StartTime) startPoint;
        public Dictionary<string, Type> DialogTypes = new()
        {
            { "welcome", typeof(WelcomeDialog) },
            { "daily-reminder", typeof(DailyReminderDialog) },
            { "expiredprofile", typeof(ExpiredProfileDialog) },
            { "soonexpireprofile", typeof(SoonExpiredProfileDialog) },
            { "add-contact", typeof(AddContactDialog) }
        };

        public Notification NextNotification
        {
            get
            {
                if (_next == null)
                    _next = NotificationClient?.Notifications?.FirstOrDefault();

                return _next;
            }
        }

        //protected override async Task OnInitializedAsync()
        //{
        //    var authState = await authenticationStateTask;
        //    User = authState.User;

        //    if (User.Identity.IsAuthenticated)
        //    {
        //        Console.WriteLine($"start get notification for user {User.Identity.Name}");

        //        var getNotifications = await httpClient.GetAsync("api/Notification");
        //        getNotifications.EnsureSuccessStatusCode();

        //        var json = await getNotifications.Content.ReadAsStringAsync();

        //        NotificationClient = new NotificationClient(NavigationManager.ToAbsoluteUri("/notification").AbsoluteUri);
        //        await NotificationClient.StartAsync();
        //        NotificationClient.NotificationEvent += TestNotification;

        //        if (!string.IsNullOrEmpty(json))
        //        {
        //            var temp = JsonConvert.DeserializeObject<List<Notification>>(json);
        //            Console.WriteLine($"getted notifications {temp.Count}");
        //            await NotificationClient.SendNotifications(temp);
        //        } 
        //    }
        //}


        public async Task SetDisplayedNotification()
        {
            //_next.IsDisplayed = true;
            //_next.DisplayCount += 1;
            //_next.LastDisplayTime = DateTime.UtcNow;
            //var post = await httpClient.PutAsJsonAsync<Notification>("api/notification", _next);
            //post.EnsureSuccessStatusCode();
            //bool uno = NotificationClient.Notifications.Remove(_next); 
            _next = null;
        }

        private async Task NextCard()
        {
            _next.IsDisplayed = true;
            _next.DisplayCount += 1;
            _next.LastDisplayTime = DateTime.UtcNow;
            var post = await httpClient.PutAsJsonAsync<Notification>("api/notification", _next);
            post.EnsureSuccessStatusCode();

            var tmp = NotificationClient.Notifications.IndexOf(_next);
            NotificationClient.Notifications.Remove(_next);

            if (tmp < NotificationClient.Notifications.Count)
            {
                _next = NotificationClient.Notifications[tmp];
            } 
            StateHasChanged();
        }
        private void HandleTouchStart(TouchEventArgs args)
        {
            startPoint.ReferencePoint = args.TargetTouches[0];
            startPoint.StartTime = DateTime.Now;
        }
        private async Task HandleTouchEnd(TouchEventArgs args)
        {
            var endReference = args.ChangedTouches[0];
            var endTime = DateTime.Now;

            var diffX = startPoint.ReferencePoint.ClientX - endReference.ClientX;
            var diffTime = DateTime.Now - startPoint.StartTime;
            var velocityX = Math.Abs(diffX / diffTime.Milliseconds);

            var swipeThreshold = 0.8;

            if (velocityX < swipeThreshold) return;

            if (velocityX >= swipeThreshold)
            {
                await NextCard();
            }
            StateHasChanged();
        }
        public void Goto(string url)
        {
            SetDisplayedNotification().ContinueWith(t => NavigationManager.NavigateTo(url, true));
        }

        public RenderFragment RenderDialog(string type) => builder =>
        {
            builder.OpenComponent(0, DialogTypes[type]);
            builder.AddAttribute(1, "User", User);
            builder.AddAttribute(2, "Close", EventCallback.Factory.Create(this, async() => await SetDisplayedNotification()));
            builder.AddAttribute(3, "Navigate", EventCallback.Factory.Create<string>(this, (url) => Goto(url)));
            builder.CloseComponent();
        };
    }
}
