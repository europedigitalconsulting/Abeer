﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace Abeer.UI_Ads.Components
{
    public partial class CardAd
    {
        [Parameter]
        public Microsoft.Extensions.Localization.IStringLocalizer Loc { get; set; }
        [Parameter]
        public AdViewModel Ad { get; set; }
        [Parameter]
        public bool Editable { get; set; }
        [Parameter]
        public EventCallback<MouseEventArgs> OnEditClicked { get; set; }
        [Parameter]
        public EventCallback<MouseEventArgs> OnDeleteClicked { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        public AuthenticationState AuthenticateSate { get; set; }
        public ViewApplicationUser User { get; set; } = new ViewApplicationUser();
        [Inject] private HttpClient HttpClient { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AuthenticateSate = await authenticationStateTask;

            if (AuthenticateSate.User.Identity.IsAuthenticated)
                User = AuthenticateSate.User;

            await base.OnInitializedAsync();
        }

        public async Task DisplayDetail()
        {
            NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri($"/ads/details/{Ad.Id}").ToString(), true);
            await InvokeAsync(StateHasChanged);
        }
        public async Task Delete()
        { 
            await OnDeleteClicked.InvokeAsync();
        }
        public async Task Update()
        { 
            await OnEditClicked.InvokeAsync();
        }
        public async Task GoToProfilAd()
        {
            NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri($"/viewprofile/{Ad.OwnerId}").ToString(), true);
        }
    }
}