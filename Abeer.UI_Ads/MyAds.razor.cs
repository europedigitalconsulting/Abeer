using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Abeer.Shared.Functional;
using Newtonsoft.Json;
using Abeer.Shared.ViewModels;

namespace Abeer.UI_Ads
{
    public partial class MyAds
    {
        private List<AdViewModel> Ads = new List<AdViewModel>();
        private List<AdViewModel> AdsTmp = new List<AdViewModel>();

        private bool ModalEditAdVisible;
        private AdViewModel Current = new AdViewModel();
        private bool UpdateHasError;
        private bool ModalDeleteAdVisible;
        private bool IsMyAds;

        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        public AuthenticationState AuthenticateSate { get; set; }
        public ViewApplicationUser User { get; set; } = new ViewApplicationUser();

        protected override async Task OnInitializedAsync()
        {
            AuthenticateSate = await authenticationStateTask;

            if (AuthenticateSate.User.Identity.IsAuthenticated)
                User = AuthenticateSate.User;

            var getAds = await HttpClient.GetAsync("/api/Ads");
            getAds.EnsureSuccessStatusCode();
            var json = await getAds.Content.ReadAsStringAsync();
            Ads = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdViewModel>>(json);
            AdsTmp = Ads.Where(x => x.OwnerId != User.Id).ToList();
            await base.OnInitializedAsync();
        }

        private void OpenEditModal(AdViewModel ad)
        {
            Current = ad;
            ModalEditAdVisible = true;
        }

        private void OpenDeleteModal(AdViewModel adModel)
        {
            Current = adModel;
            ModalDeleteAdVisible = true;
        }

        private async Task Delete()
        {
            var update = await HttpClient.DeleteAsync($"/api/Ads/{Current.Id}");
            update.EnsureSuccessStatusCode();
            ModalDeleteAdVisible = false;
            Ads.Remove(Current);
            await InvokeAsync(StateHasChanged);
        }

        private async Task ViewNotValid()
        {
            var getAds = await HttpClient.GetAsync("/api/Ads/notvalid");
            getAds.EnsureSuccessStatusCode();
            var json = await getAds.Content.ReadAsStringAsync();
            Ads = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdViewModel>>(json);
            await InvokeAsync(StateHasChanged);
        }

        private async Task GotoAll()
        {
            NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri("ads/list").ToString(), true);
            await InvokeAsync(StateHasChanged);
        }
        private async Task FilterMyAds()
        { 
            IsMyAds = true;
            AdsTmp = Ads.Where(x => x.OwnerId == User.Id).ToList();
            await InvokeAsync(StateHasChanged);
        }
        private async Task ResetListAds()
        {
            IsMyAds = false;
            AdsTmp = Ads;
            await InvokeAsync(StateHasChanged);
        }
    }
}
