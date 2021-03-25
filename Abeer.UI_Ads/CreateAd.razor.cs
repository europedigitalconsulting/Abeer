using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace Abeer.UI_Ads
{
    public partial class CreateAd
    {
        public string Step { get; set; } = "Step1";
        private AdViewModel Ad { get; set; } = new AdViewModel();
        private AdPrice CurrentPrice { get; set; } = new AdPrice();
        private List<AdPrice> AdPrices { get; set; } = new List<AdPrice>(); 
        private bool PublishHasError { get; set; }
        private AdPaymentOption AdPaymentOption { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        public AuthenticationState AuthenticateSate { get; set; } 
        public bool ShowCategory { get; set; } = false;
        public bool ShowFamily { get; set; } = false;
        public string FamilySearch { get; set; }
        public AdsFamilyViewModel FamilySelected { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }
        protected List<AdsFamilyViewModel> ListFamily { get; set; } = new List<AdsFamilyViewModel>();
        protected List<AdsFamilyViewModel> ListFamilyTmp { get; set; } = new List<AdsFamilyViewModel>();
        protected List<AdsCategoryViewModel> ListCateg { get; set; } = new List<AdsCategoryViewModel>();
        protected override async Task OnInitializedAsync()
        {
            AuthenticateSate = await authenticationStateTask;

            var getListFamily = await HttpClient.GetAsync($"/api/bo/Families/FamiliesBy/{Ad.Id}");
            getListFamily.EnsureSuccessStatusCode();
            var json = await getListFamily.Content.ReadAsStringAsync();
            ListFamilyTmp = ListFamily = JsonConvert.DeserializeObject<List<AdsFamilyViewModel>>(json);
            FamilySelected = ListFamilyTmp.FirstOrDefault(x => x.Categories.Any(c => c.Selected));
            if (FamilySelected != null)
            {
                FamilySearch = FamilySelected.Label;
                ListCateg = FamilySelected.Categories;
                ShowCategory = true;
            }
            await InvokeAsync(StateHasChanged);

            var getAll = await HttpClient.GetAsync("/api/AdPrice/GetFeature");
            
            if (getAll.IsSuccessStatusCode)
            {
                 json = await getAll.Content.ReadAsStringAsync();
                AdPaymentOption = JsonConvert.DeserializeObject<AdPaymentOption>(json);
            }

            await InvokeAsync(StateHasChanged);
            await base.OnInitializedAsync();
        }

        private async Task Step1()
        {
            Step = "Step1";
            await InvokeAsync(StateHasChanged);
        }

        private async Task Step2()
        {
            Step = "Step2";

            var getAll = await HttpClient.GetAsync("/api/AdPrice");
            if (getAll.IsSuccessStatusCode)
            {
                var json = await getAll.Content.ReadAsStringAsync();
                AdPrices = JsonConvert.DeserializeObject<List<AdPrice>>(json);
            }

            var adPrice = AdPrices.FirstOrDefault(p => p.Value == 0);
            await SelectPrice(adPrice);
        }

        private async Task SelectPrice(AdPrice currentPrice)
        {
            CurrentPrice = currentPrice;
            Step = "Step3";
            await InvokeAsync(StateHasChanged);
        }

        private async Task ValidAd()
        {
            var CreateAdRequest = new CreateAdRequestViewModel
            {
                Ad = Ad,
                Price = CurrentPrice
            };

            var response = await InsertAd(CreateAdRequest);

            if (!PublishHasError)
            {
                var json = await response.Content.ReadAsStringAsync();
                var Ad = JsonConvert.DeserializeObject<AdViewModel>(json);
                NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri($"/ads/ValidAd/{Ad.Id}").ToString(), true);
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task<HttpResponseMessage> InsertAd(CreateAdRequestViewModel CreateAdRequest)
        {
            var response = await HttpClient.PostAsJsonAsync<CreateAdRequestViewModel>("/api/Ads", CreateAdRequest);
            PublishHasError = !response.IsSuccessStatusCode;
            return response;
        }

        protected async Task CreatePayment(string methodPayment)
        {
            var CreateAdRequest = new CreateAdRequestViewModel
            {
                Ad = Ad,
                Price = CurrentPrice
            };

            var response = await InsertAd(CreateAdRequest);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Ad = JsonConvert.DeserializeObject<AdViewModel>(json);
                NavigationManager.NavigateTo($"/payment/{methodPayment}/{Ad.OrderNumber}");
            }
        }
        private void CheckboxChanged(ChangeEventArgs e, string key)
        {
            var i = ListCateg.FirstOrDefault(i => i.Code == key);
            if (i != null)
            {
                i.Selected = (bool)e.Value;
            }
            Ad.ListIdCategory = ListCateg.Where(c => c.Selected).Select(x => x.CategoryId).ToList(); ;
        }
        private void SelectFamily(AdsFamilyViewModel family)
        {
            family.Categories.ForEach(x => x.Selected = false);
            FamilySelected = family;
            FamilySearch = FamilySelected.Label;
            ListCateg = family.Categories;
            ShowFamily = false;
            ShowCategory = true;
            StateHasChanged();
        }
        void TapSearch(ChangeEventArgs e)
        {
            FamilySearch = e.Value.ToString();
            ListFamily = ListFamilyTmp.Where(x => x.Label.StartsWith(FamilySearch, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        void FocusSearchFamily()
        {
            ShowFamily = !ShowFamily;
            ShowCategory = false;
        }
        private void AssignImageUrl1(string imgUrl) => Ad.ImageUrl1 = imgUrl;
        private void AssignImageUrl2(string imgUrl) => Ad.ImageUrl2 = imgUrl;
        private void AssignImageUrl3(string imgUrl) => Ad.ImageUrl3 = imgUrl;
        private void AssignImageUrl4(string imgUrl) => Ad.ImageUrl4 = imgUrl;
    }
}
