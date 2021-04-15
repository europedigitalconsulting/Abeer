﻿using Abeer.Client.Shared.NotificationDialogs;
using Abeer.Shared.ClientHub;
using Abeer.Shared.EventNotification;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
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
using Abeer.Shared.ViewModels;

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
        public Dictionary<NotificationTypeEnum, Type> DialogTypes = new()
        {
            { NotificationTypeEnum.Welcome, typeof(WelcomeDialog) },
            { NotificationTypeEnum.DailyReminder, typeof(DailyReminderDialog) },
            { NotificationTypeEnum.ExpiredProfile, typeof(ExpiredProfileDialog) },
            { NotificationTypeEnum.SoonExpireProfile, typeof(SoonExpiredProfileDialog) },
            { NotificationTypeEnum.AddContact, typeof(AddContactDialog) },
            { NotificationTypeEnum.AdStartPublished, typeof(StartPublishingDialog) },
            { NotificationTypeEnum.AdEndPublished, typeof(EndPublishingDialog) },
            { NotificationTypeEnum.RemoveContact, typeof(RemoveContactDialog)},
            { NotificationTypeEnum.SendProfile, typeof(SendProfileDialog) },
            { NotificationTypeEnum.ValidateAd, typeof(ValidateAdDialog) }
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
            if (_next != null)
            {
                _next.IsDisplayed = true;
                _next.DisplayCount += 1;
                _next.LastDisplayTime = DateTime.UtcNow;
                var post = await httpClient.PutAsJsonAsync<Notification>("api/notification", _next);
                post.EnsureSuccessStatusCode();
                Console.WriteLine($"before remove {NotificationClient.Notifications.Count}");
                NotificationClient.Notifications.Remove(_next);
                Console.WriteLine($"After remove {NotificationClient.Notifications.Count}");
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task NextCard()
        {

            await SetDisplayedNotification();

            if (NotificationClient.Notifications.Count < 1)
                return;

            var tmp = NotificationClient.Notifications.IndexOf(_next);
            
            if (tmp < NotificationClient.Notifications.Count)
            {
                _next = NotificationClient.Notifications[tmp];
            } 
            
            await InvokeAsync(StateHasChanged);
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
        public async Task Goto(string url)
        {
            await SetDisplayedNotification();

            NavigationManager.NavigateTo(url, true);
        }

        public RenderFragment RenderDialog(string type) => builder =>
        {
            builder.OpenComponent(0, DialogTypes[type.ConvertAsNotificationType()]);
            builder.AddAttribute(1, "User", User);
            builder.AddAttribute(2, "Close", EventCallback.Factory.Create(this, async() => await SetDisplayedNotification()));
            builder.AddAttribute(3, "Navigate", EventCallback.Factory.Create<string>(this, (url) => Goto(url)));
            builder.AddAttribute(4, "Notification", _next);
            builder.CloseComponent();
        };
    }
}
