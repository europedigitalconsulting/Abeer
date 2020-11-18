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
            Console.WriteLine(OrderNumber);
            if (!string.IsNullOrEmpty(OrderNumber))
            {
                var response = await HttpClient.PostAsJsonAsync($"{DomainApiPayment}/api/Payment/GetAccessToken", new
                {
                    ClientId,
                    ClientSecret,
                    Price,
                    OrderNumber,
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
                    string OrderNumberReturn = await response.Content.ReadAsStringAsync();
                    OrderNumberReturn = JsonConvert.DeserializeObject<string>(OrderNumberReturn);
                    NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri($"{DomainApiPayment}/Payment/{OrderNumberReturn}").ToString(), true);
                }
            }
            else
                NavigationManager.NavigateTo(RedirectError, true);
        } 
    }
}
