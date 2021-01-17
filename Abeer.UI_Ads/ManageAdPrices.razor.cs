using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace Abeer.UI_Ads
{
    public partial class ManageAdPrices
    {
        bool ModalFormAdVisible;
        bool FormHasError;

        string TitleForm = "EditForm";
        string Mode = "Insert";

        Abeer.Shared.Functional.AdPrice Current = new Abeer.Shared.Functional.AdPrice();

        public string Term { get; set; }

        List<Abeer.Shared.Functional.AdPrice> All = new List<Abeer.Shared.Functional.AdPrice>();
        List<Abeer.Shared.Functional.AdPrice> Items = new List<Abeer.Shared.Functional.AdPrice>();

        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject] private NavigationManager navigationManager { get; set; }

        void countTerm(KeyboardEventArgs e)
        {
            if (Term.Length > 5)
                Search();
        }

        protected override async Task OnInitializedAsync()
        {
            var authenticateSate = await authenticationStateTask;

            Console.WriteLine("User claims");

            if (!authenticateSate.User.Identity.IsAuthenticated || !authenticateSate.User.HasClaim(ClaimTypes.Role, "admin"))
            {
                Console.WriteLine("User is not admin, redirect to login");
                navigationManager.NavigateTo("/authentication/Login", true);
            }

            var httpClient = HttpClientFactory.CreateClient("Abeer.Anonymous");
            var getAll = await httpClient.GetAsync("/api/AdPrice");
            getAll.EnsureSuccessStatusCode();
            var json = await getAll.Content.ReadAsStringAsync();
            All = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Abeer.Shared.Functional.AdPrice>>(json);
            Items = All.ToList();
        }

        void Search()
        {
            Items = All.Where(a => a.PriceName.Contains(Term) || a.PriceDescription.Contains(Term)).ToList();
        }

        void OpenEditModal(Abeer.Shared.Functional.AdPrice AdPrice)
        {
            Current = AdPrice;
            Mode = "Edit";
            ModalFormAdVisible = true;
        }

        void OpenDeleteModal(Abeer.Shared.Functional.AdPrice AdPrice)
        {
            Current = AdPrice;
            Mode = "Delete";
            ModalFormAdVisible = true;
        }

        void OpenCreateAd()
        {
            Current = new Abeer.Shared.Functional.AdPrice();
            Mode = "Insert";
            ModalFormAdVisible = true;
        }

        async Task Save()
        {
            switch (Mode)
            {
                case "Insert":
                    {
                        var postResponse = await HttpClient.PostAsJsonAsync<Abeer.Shared.Functional.AdPrice>("/api/AdPrice", Current);
                        postResponse.EnsureSuccessStatusCode();
                        All.Add(Current);
                        Items.Add(Current);
                        Current = new Abeer.Shared.Functional.AdPrice();
                        ModalFormAdVisible = false;
                        break;
                    }
                case "Edit":
                    {
                        var postResponse = await HttpClient.PutAsJsonAsync<Abeer.Shared.Functional.AdPrice>("/api/AdPrice", Current);
                        postResponse.EnsureSuccessStatusCode();
                        Current = new Abeer.Shared.Functional.AdPrice();
                        ModalFormAdVisible = false;
                        break;
                    }
                case "Delete":
                    {
                        var postResponse = await HttpClient.DeleteAsync($"/api/AdPrice");
                        postResponse.EnsureSuccessStatusCode();
                        Current = new Abeer.Shared.Functional.AdPrice();
                        ModalFormAdVisible = false;
                        break;
                    }
            }
        }
        bool IsFormDisabled => Mode == "Delete";
    }
}
