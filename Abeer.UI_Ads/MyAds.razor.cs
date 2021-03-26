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
using Microsoft.JSInterop;
using Abeer.Ads.Shared;

namespace Abeer.UI_Ads
{
    public partial class MyAds
    {
        private List<AdViewModel> AdsTmp = new List<AdViewModel>();

        private bool ModalEditAdVisible;
        private bool ModalDeleteAdVisible;
        private bool UpdateHasError;
        private AdViewModel Current = new AdViewModel();
        private AdsFamilyViewModel FamilySelected;
        private MyAdsViewModel RefAds = new MyAdsViewModel();
        private bool IsMyAds;
        private bool ShowFilter;
        private bool ShowFilter2;
        private bool ShowFilterCountry;
        private bool ShowFilterCategory;
        private string searchTxt;

        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        public AuthenticationState AuthenticateSate { get; set; }
        public ViewApplicationUser User { get; set; } = new ViewApplicationUser();

        protected override async Task OnInitializedAsync()
        {
            await jSRuntime.InvokeVoidAsync("outsideClickHandler", "dropdownmenu", DotNetObjectReference.Create(this));
            AuthenticateSate = await authenticationStateTask;

            if (AuthenticateSate.User.Identity.IsAuthenticated)
                User = AuthenticateSate.User;

            var getAds = await HttpClient.GetAsync("/api/Ads");
            getAds.EnsureSuccessStatusCode();
            var json = await getAds.Content.ReadAsStringAsync();
            RefAds = JsonConvert.DeserializeObject<MyAdsViewModel>(json);
            AdsTmp = RefAds.Ads.Where(x => x.OwnerId != User.Id).ToList();
            await base.OnInitializedAsync();
        }
        [JSInvokable]
        public void InvokeClickOutside()
        {
            if (ShowFilter2 && ShowFilter)
            {
                ShowFilter = ShowFilter2 = false;
            }
            else
                ShowFilter = true;
            StateHasChanged();
        }
        public void OpenFilter()
        {
            ShowFilter2 = true;
            StateHasChanged();
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
            RefAds.Ads.Remove(Current);
            AdsTmp = RefAds.Ads;
            await InvokeAsync(StateHasChanged);
        }

        private async Task FilterMyAds()
        {
            IsMyAds = true;
            AdsTmp = RefAds.Ads.Where(x => x.OwnerId == User.Id).ToList();
            await InvokeAsync(StateHasChanged);
        }
        private async Task ResetListAds()
        {
            IsMyAds = false;
            AdsTmp = RefAds.Ads;
            await InvokeAsync(StateHasChanged);
        }
        private async Task OpenFilterCountry()
        {
            ShowFilterCountry = true;
            ShowFilterCategory = false;
            await InvokeAsync(StateHasChanged);
        }
        private async Task OpenFilterCategory(AdsFamilyViewModel model)
        {
            RefAds.Families.ForEach(x => x.Categories.ForEach(f => f.Selected = false));
            FamilySelected = model;
            ShowFilterCountry = false;
            ShowFilterCategory = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task SelectCategory(AdsCategoryViewModel category)
        {
            category.Selected = !category.Selected;
            await RefeshResultFilter();
        }
        private async Task RefeshResultFilter()
        {
            MyAdsViewModel x = new MyAdsViewModel();

            x.ListCodeCountrySelected = RefAds?.Countries?.Where(x => x.Selected).Select(c => c.Eeacode).ToList() ?? new List<string>();
            x.ListIdCategorySelected = FamilySelected?.Categories.Where(x => x.Selected).Select(c => c.CategoryId).ToList() ?? new List<Guid>();
            x.searchTxt = searchTxt ?? "";

            var getFamily = await HttpClient.PostAsJsonAsync($"/api/Ads/search", x);
            getFamily.EnsureSuccessStatusCode();
            var json = await getFamily.Content.ReadAsStringAsync();

            AdsTmp = RefAds.Ads = JsonConvert.DeserializeObject<List<AdViewModel>>(json);

            await InvokeAsync(StateHasChanged);
        }
        private async Task SelectCountry(CountryViewModel country)
        {
            country.Selected = !country.Selected;
            await RefeshResultFilter();
        }
        private async Task TapSearch(ChangeEventArgs e)
        { 
            searchTxt = e.Value.ToString();
            await RefeshResultFilter();
        }
    }
}
