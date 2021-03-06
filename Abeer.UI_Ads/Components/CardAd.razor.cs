﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Abeer.UI_Ads.Components
{
    public partial class CardAd
    {
        [Parameter]
        public Microsoft.Extensions.Localization.IStringLocalizer Loc { get; set; }
        [Parameter]
        public Abeer.Shared.Functional.AdModel Ad { get; set; }
        [Parameter]
        public Boolean Editable { get; set; }
        [Parameter]
        public EventCallback<MouseEventArgs> OnEditClicked { get; set; }
        [Parameter]
        public EventCallback<MouseEventArgs> OnDeleteClicked { get; set; }

        public async  Task DisplayDetail()
        {
            NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri($"/ads/details/{Ad.Id}").ToString(), true);
            await InvokeAsync(StateHasChanged);
        }
    }
}