
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
        public List<Batch> Cards { get; set; } = new List<Batch>();
        protected string SearchTerm { get; set; }
        protected List<Batch> Items = new List<Batch>();

        [Inject]
        public HttpClient HttpClient { get; set; }

        [Inject] private NavigationManager navigationManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        IJSRuntime ThisJSRuntime { get; set; }

        ClaimsPrincipal User;

        async Task DownloadDocument(Batch batch)
        {
            await ThisJSRuntime.InvokeVoidAsync(
                "downloadFromByteArray",
                new
                {
                    ByteArray = Convert.ToBase64String(batch.CsvFileContent),
                    FileName = $"data_{batch.Id.ToString()}.csv",
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
                Cards = JsonConvert.DeserializeObject<List<Batch>>(json);
                Items = Cards.OrderBy(c=>c.CardType).ToList();

                Console.WriteLine(User?.HasClaim(System.Security.Claims.ClaimTypes.Role, "manager") == true ? "user is manager" : "");
                Console.WriteLine(User?.HasClaim(System.Security.Claims.ClaimTypes.Role, "admin") == true ? "user is admin" : "");
            }
        }

        public void SearchButtonClick()
        {
            Console.WriteLine($"start search Card {SearchTerm}");
            
            var found = Cards?.Where(c => c.CardType?.Contains(SearchTerm) == true)?.ToList();

            Items =  found.OrderBy(c=>c.CardType).ToList();
            Console.WriteLine($"{found.Count} Token Batches found");
            StateHasChanged();
        }
        void CloseModal() => IsModalVisible = false;
        void OpenModal() => IsModalVisible = true;

        string Mode { get; set; } = "Insert";

        Batch current = null;

        static readonly Random rdm = new Random();

        void ShowInsertCard()
        {
            current = new Batch
            {
                Quantity = 1
            };

            Mode = "Insert";
            OpenModal();
            StateHasChanged();
        }
        void ShowEditCard(Batch batch)
        {
            current = batch;
            Mode = "Update";
            OpenModal();
            StateHasChanged();
        }

        void ShowDeleteCard(Batch batch)
        {
            current = batch;
            Mode = "Delete";
            OpenModal();
        }
        async Task Insert()
        {
            Console.WriteLine(current);
            var response = await HttpClient.PostAsJsonAsync<Batch>("api/Card", current);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Cards = JsonConvert.DeserializeObject<List<Batch>>(json);
            Items = Cards.OrderBy(c => c.CardType).ToList();
            IsModalVisible = false;
            await InvokeAsync(StateHasChanged);
        }

        async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync<Batch>($"api/Card/{current.Id}", current);
            response.EnsureSuccessStatusCode();
            IsModalVisible = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
