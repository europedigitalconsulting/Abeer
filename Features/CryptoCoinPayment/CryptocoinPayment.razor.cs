using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Cryptocoin.Payment
{
    public partial class CryptocoinPayment : ComponentBase
    {
        [Inject] protected HttpClient HttpClient { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        private async Task ValidPayment()
        {
            await BeforeCallPayment.InvokeAsync(null);

            if (!string.IsNullOrEmpty(OrderNumber))
            {
                var response = await HttpClient.PostAsJsonAsync($"{CryptoConfig.DomainApiPayment}/api/Payment/GetAccessToken", new
                {
                    CryptoConfig.ClientId,
                    CryptoConfig.ClientSecret,
                    CryptoConfig.RedirectSuccessServer,
                    CryptoConfig.RedirectErrorServer,
                    CryptoConfig.RedirectSuccess,
                    CryptoConfig.RedirectError,
                    Price,
                    OrderNumber,
                    Items
                });

                if (!response.IsSuccessStatusCode)
                {
                       NavigationManager.NavigateTo($"/{PagePaymentError}", true);
                }
                else
                {
                    string OrderNumberReturn = await response.Content.ReadAsStringAsync();
                    OrderNumberReturn = JsonConvert.DeserializeObject<string>(OrderNumberReturn); 
                    NavigationManager.NavigateTo($"{CryptoConfig.DomainApiPayment}/Payment/{OrderNumberReturn}", true);
                }
            }
            else
            {
                NavigationManager.NavigateTo("error", true);
            }
        }
    }
}
