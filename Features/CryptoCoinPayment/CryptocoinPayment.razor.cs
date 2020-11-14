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
        //[Parameter] public EventCallback<string> CallbackGetIdOrder { get; set; }
        [Inject] protected HttpClient HttpClient { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        private async Task ValidPayment()
        {
            var response = await HttpClient.PostAsJsonAsync($"{DomainApiPayment}/api/Payment/GetAccessToken", new
            {
                ClientId,
                ClientSecret,
                PriceEuro = Price,
                RedirectSuccessServer,
                RedirectErrorServer,
                RedirectSuccess,
                RedirectError,
                Items
            });

            if (!response.IsSuccessStatusCode)
            {
                NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri(RedirectError).ToString(), true);
            }
            else
            {
                string OrderId = await response.Content.ReadAsStringAsync();
                OrderId = JsonConvert.DeserializeObject<string>(OrderId);
                NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri($"{DomainApiPayment}/Payment/{OrderId}").ToString(), true);
            }

        } 
    }
}
