using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        private AdModel Ad { get; set; } = new AdModel();
        private AdPrice CurrentPrice { get; set; } = new AdPrice();
        private List<AdPrice> AdPrices { get; set; } = new List<AdPrice>();
        private bool PublishHasError { get; set; }
        private AdPaymentOption AdPaymentOption { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        public AuthenticationState AuthenticateSate { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AuthenticateSate = await authenticationStateTask;

            var httpClient = HttpClientFactory.CreateClient("Abeer.ServerAPI");
            var getAll = await httpClient.GetAsync("/api/AdPrice/GetFeature");
            
            if (getAll.IsSuccessStatusCode)
            {
                var json = await getAll.Content.ReadAsStringAsync();
                AdPaymentOption = JsonConvert.DeserializeObject<AdPaymentOption>(json);
            }

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

            var httpClient = HttpClientFactory.CreateClient("Abeer.ServerAPI");
            var getAll = await httpClient.GetAsync("/api/AdPrice");
            if (getAll.IsSuccessStatusCode)
            {
                var json = await getAll.Content.ReadAsStringAsync();
                AdPrices = JsonConvert.DeserializeObject<List<AdPrice>>(json);
            }

            await InvokeAsync(StateHasChanged);
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
                var Ad = JsonConvert.DeserializeObject<AdModel>(json);
                NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri($"/ads/ValidAd/{Ad.Id}").ToString(), true);
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task<HttpResponseMessage> InsertAd(CreateAdRequestViewModel CreateAdRequest)
        {
            var httpClient = HttpClientFactory.CreateClient("Abeer.ServerAPI");
            var response = await httpClient.PostAsJsonAsync<CreateAdRequestViewModel>("/api/Ads", CreateAdRequest);
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
                Ad = JsonConvert.DeserializeObject<AdModel>(json);
                NavigationManager.NavigateTo($"/payment/{methodPayment}/{Ad.OrderNumber}");
            }
        }

        private void AssignImageUrl1(string imgUrl) => Ad.ImageUrl1 = imgUrl;
        private void AssignImageUrl2(string imgUrl) => Ad.ImageUrl2 = imgUrl;
        private void AssignImageUrl3(string imgUrl) => Ad.ImageUrl3 = imgUrl;
        private void AssignImageUrl4(string imgUrl) => Ad.ImageUrl4 = imgUrl;
    }
}
