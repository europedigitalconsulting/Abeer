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

namespace Abeer.UI_Ads
{
    public partial class MyAds
    {
        private List<AdModel> Ads = new List<AdModel>();
        private List<AdModel> AdsTmp = new List<AdModel>();

        private bool ModalEditAdVisible;
        private AdModel Current = new AdModel();
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
            Ads = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdModel>>(json);
            AdsTmp = Ads.Where(x => x.OwnerId != User.Id).ToList();
            await base.OnInitializedAsync();
        }

        private void OpenEditModal(AdModel ad)
        {
            Current = ad;
            ModalEditAdVisible = true;
        }

        private void OpenDeleteModal(AdModel adModel)
        {
            Current = adModel;
            ModalDeleteAdVisible = true;
        }

        private async Task Update()
        {
            var update = await HttpClient.PutAsJsonAsync<AdModel>("/api/Ads", Current);
            update.EnsureSuccessStatusCode();
            ModalEditAdVisible = false;
            await InvokeAsync(StateHasChanged);
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
            Ads = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdModel>>(json);
            await InvokeAsync(StateHasChanged);
        }

        private async Task GotoAll()
        {
            NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri("ads/list").ToString(), true);
            await InvokeAsync(StateHasChanged);
        }
        private async Task FilterMyAds()
        {
            Console.WriteLine(User.Id);
            Console.WriteLine(JsonConvert.SerializeObject(Ads));
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
