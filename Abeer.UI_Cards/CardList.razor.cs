using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Abeer.UI_Cards
{
    public partial class CardList : ComponentBase
    {
        bool IsModalVisible = false;

        [Parameter]
        public  IStringLocalizer Localizer { get; set; }

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

        async Task DownloadDocument()
        {
            await ThisJSRuntime.InvokeVoidAsync(
                "downloadFromByteArray",
                new
                {
                    ByteArray = Convert.ToBase64String(Batch.CsvFileContent),
                    FileName = $"data_{Batch.Id}_{DateTime.UtcNow.ToString("yyyMMddHHmmss") + ".xlsx"}",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
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
                Items = Cards.ToList();

                Console.WriteLine(User?.HasClaim(System.Security.Claims.ClaimTypes.Role, "manager") == true ? "user is manager" : "");
                Console.WriteLine(User?.HasClaim(System.Security.Claims.ClaimTypes.Role, "admin") == true ? "user is admin" : "");
            }
        }

        public void SearchButtonClick()
        {
            Console.WriteLine($"start search Current {SearchTerm}");
            
            var found = Cards?.Where(c => c.CardNumber?.Contains(SearchTerm) == true)?.ToList();

            Items = found;
            Console.WriteLine($"{found.Count} Token Batches found");
            StateHasChanged();
        }

        public  bool IsBatchModalVisible { get; set; }

        void CloseModal() => IsModalVisible = false;
        void OpenModal() => IsModalVisible = true;
        void OpenBatchModal() => IsBatchModalVisible = true;
        void CloseBatchmodal() => IsBatchModalVisible = false;

        public Batch Batch { get; private set; }
        string Mode { get; set; } = "Insert";

        private bool IsBatchGenerated { get; set; }

        Card current = null;

        static readonly Random rdm = new Random();

        void ShowInsertCard()
        {
            current = new Card
            {
                CardNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100000, 999999)),
                PinCode = rdm.Next(10000, 99999).ToString()
            };

            Mode = "Insert";
            OpenModal();

            StateHasChanged(); 
        }

        void ShowInsertBatch()
        {
            Batch = new Batch()
            {
                CardStartNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), "100000"),
                Quantity =  1
            };

            Mode = "Insert";
            OpenBatchModal();

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
            var response = await HttpClient.PostAsJsonAsync<Card>("api/Card", current);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"insert result : {json}");
            var Card = JsonConvert.DeserializeObject<Card>(json);
            Cards.Add(Card);
            Items.Add(Card);
            CloseModal();
            await InvokeAsync(StateHasChanged);
        }

        async Task Generate()
        {
            var response = await HttpClient.PostAsJsonAsync<Batch>("api/Card/generate", Batch);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var generated = JsonConvert.DeserializeObject<GeneratedBatchViewModel>(json);

            Cards.AddRange(generated.Cards);
            Items.AddRange(generated.Cards);
            Batch = generated.Batch;

            CloseBatchmodal();
            IsBatchGenerated = true;
            await InvokeAsync(StateHasChanged);
        }

        async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync<Card>($"api/Card/{current.Id}", current);
            response.EnsureSuccessStatusCode();
            CloseModal();
            StateHasChanged();
        }

        async Task SellCard(Card card)
        {
            current = card;
            current.IsSold = true;
            current.SoldDate = DateTime.UtcNow;
            var response = await HttpClient.PutAsJsonAsync<Card>($"api/Card/sell/{current.Id}", current);
            response.EnsureSuccessStatusCode();
            CloseModal();
            StateHasChanged();
        }

        async Task Delete()
        {
            var response = await HttpClient.DeleteAsync($"api/Card/{current.Id}");
            response.EnsureSuccessStatusCode();
            Cards.Remove(current);
            Items.Remove(current);
            CloseModal();
            StateHasChanged();
        }

        Task Save()
        {
            switch (Mode)
            {
                case "Insert":
                    return Insert();
                case "Update":
                    return Update();
            }

            return Task.CompletedTask;
        }
    }
}
