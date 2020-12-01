
using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class CardList : ComponentBase
    {
        bool IsModalVisible = false;

        protected CardForm CardForm { get; set; }

        public string TitleDialog { get; set; }
        public List<Card> Cards { get; set; } = new List<Card>();
        protected string SearchTerm { get; set; }
        protected List<Card> Items = new List<Card>();

        [Inject]
        public HttpClient HttpClient { get; set; }

        [Inject] private NavigationManager navigationManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        IJSRuntime ThisJSRuntime { get; set; }

        ClaimsPrincipal User;

        async Task DownloadDocument(Card Card)
        {
            await ThisJSRuntime.InvokeVoidAsync(
                "downloadFromByteArray",
                new
                {
                    ByteArray = Convert.ToBase64String(Card.CsvFileContent),
                    FileName = $"data_{Card.CardNumber}_{DateTime.UtcNow.ToString("yyyMMddHHmmss") + ".csv"}",
                    ContentType = "text/csv"
                });
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            var authenticateSate = await authenticationStateTask;

            User = authenticateSate.User;

            if (!User.Identity.IsAuthenticated
                || (!User.HasClaim(ClaimTypes.Role, "operator")
                    && !User.HasClaim(ClaimTypes.Role, "admin")) 
                    && !User.HasClaim(ClaimTypes.Role, "manager"))
            {
                navigationManager.NavigateTo("/authentication/Login", true);
            }
            else
            {

                var response = await HttpClient.GetAsync("/api/Card", HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                Cards = JsonConvert.DeserializeObject<List<Card>>(json);
                Items = Cards.OrderBy(c=>c.CardNumber).ToList();

                Console.WriteLine(User?.HasClaim(System.Security.Claims.ClaimTypes.Role, "manager") == true ? "user is manager" : "");
                Console.WriteLine(User?.HasClaim(System.Security.Claims.ClaimTypes.Role, "admin") == true ? "user is admin" : "");
            }
        }

        public void SearchButtonClick()
        {
            Console.WriteLine($"start search Card {SearchTerm}");
            
            var found = Cards?.Where(c => c.CardNumber?.Contains(SearchTerm) == true)?.ToList();

            Items =  found.OrderBy(c=>c.CardNumber).ToList();
            Console.WriteLine($"{found.Count} Token Batches found");
            StateHasChanged();
        }
        void CloseModal() => IsModalVisible = false;
        void OpenModal() => IsModalVisible = true;

        string Mode { get; set; } = "Insert";

        Card current = null;

        static readonly Random rdm = new Random();

        void ShowInsertCard()
        {
            current = new Card
            {
                Quantity = 1
            };
            Mode = "Insert";
            OpenModal();
            StateHasChanged();
        }
        void ShowEditCard(Card Card)
        {
            current = Card;
            Mode = "Update";
            OpenModal();
            StateHasChanged();
        }

        void ShowDeleteCard(Card Card)
        {
            current = Card;
            Mode = "Delete";
            OpenModal();
        }
        async Task Insert()
        {
            Console.WriteLine(current);
            var response = await HttpClient.PostAsJsonAsync<Card>("api/Card", current);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"insert result : {json}"); 
            Cards = JsonConvert.DeserializeObject<List<Card>>(json);
            Items = Cards.OrderBy(c => c.CardNumber).ToList();
            IsModalVisible = false;
            await InvokeAsync(StateHasChanged);
        }

        async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync<Card>($"api/Card/{current.Id}", current);
            response.EnsureSuccessStatusCode();
            IsModalVisible = false;
            await InvokeAsync(StateHasChanged);
        }

        async Task SellCard(Card card)
        {
            current = card;
            current.IsSold = true;
            current.SoldDate = DateTime.UtcNow;
            var response = await HttpClient.PutAsJsonAsync<Card>($"api/Card/sell/{current.Id}", current);
            response.EnsureSuccessStatusCode();
            IsModalVisible = false;
            await InvokeAsync(StateHasChanged);
        }

        async Task Delete()
        {
            var response = await HttpClient.DeleteAsync($"api/Card/{current.Id}");
            response.EnsureSuccessStatusCode();
            Cards.Remove(current);
            Items.Remove(current);
            IsModalVisible = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
