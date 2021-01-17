using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;

namespace Abeer.UI_Ads
{
    public partial class ValidAd
    {
        [Parameter]
        public string AdId { get; set; }

        public string Step { get; set; } = "Step1";

        public AdModel Ad { get; set; } = new AdModel();
        public AdPrice CurrentPrice { get; set; } = new AdPrice();

        private async Task Step2()
        {
            Step = "Step2";
            await InvokeAsync(StateHasChanged);
        }


        protected override async Task OnInitializedAsync()
        {
            var response = await httpClient.GetAsync($"api/ads/{AdId}");

            if (!response.IsSuccessStatusCode)
            {
                NavigationManager.NavigateTo("CreateAd");
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ad:{json}");
                Ad = Newtonsoft.Json.JsonConvert.DeserializeObject<AdModel>(json);
                await base.OnParametersSetAsync();
            }
        }

        private async Task SelectPrice(AdPrice currentPrice)
        {
            CurrentPrice = currentPrice;
            Step = "Step3";
            await InvokeAsync(StateHasChanged);
        }

        private async Task Finish()
        {
            var response = await httpClient.GetAsync($"api/ads/valid/{Ad.Id}");
            response.EnsureSuccessStatusCode();
            NavigationManager.NavigateTo("/ads/MyAds");
        }

        private bool IsValid => Ad?.AdPrice?.Value == 0 || !string.IsNullOrEmpty(Ad?.PaymentInformation);
        private bool RequirePayment => Ad?.AdPrice?.Value > 0;
    }
}
