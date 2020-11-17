using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;

namespace Abeer.Client.Pages
{
    public partial class ManageAds : ComponentBase
    {
        bool ModalFormAdVisible;
        bool FormHasError;

        string TitleForm = "EditForm";
        string Mode = "Insert";

        Abeer.Shared.Functional.AdModel Current = new Abeer.Shared.Functional.AdModel();

        public string Term { get; set; }

        List<Abeer.Shared.Functional.AdModel> All = new List<Abeer.Shared.Functional.AdModel>();
        List<Abeer.Shared.Functional.AdModel> Items = new List<Abeer.Shared.Functional.AdModel>();

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

            var getAll = await HttpClient.GetAsync("/api/adss/admin");
            getAll.EnsureSuccessStatusCode();
            var json = await getAll.Content.ReadAsStringAsync();
            All = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Abeer.Shared.Functional.AdModel>>(json);
            if (All != null)
            {
                Items = All.ToList();
            }

        }

        void Search()
        {
            if (All != null)
                Items = All.Where(a => a.Title.Contains(Term) || a.Description.Contains(Term)).ToList();
        }

        void OpenEditModal(Abeer.Shared.Functional.AdModel adModel)
        {
            Current = adModel;
            Mode = "Edit";
            ModalFormAdVisible = true;
        }

        void OpenDeleteModal(Abeer.Shared.Functional.AdModel adModel)
        {
            Current = adModel;
            Mode = "Delete";
            ModalFormAdVisible = true;
        }

        void OpenCreateAd()
        {
            Current = new Abeer.Shared.Functional.AdModel();
            Current.StartDisplayTime = DateTime.UtcNow;
            Mode = "Insert";
            ModalFormAdVisible = true;
        }

        string FormError = "";

        async Task Save()
        {
            FormHasError = false;
            FormError = "";

            switch (Mode)
            {
                case "Insert":
                    {
                        var postResponse = await HttpClient.PostAsJsonAsync<Abeer.Shared.Functional.AdModel>("/api/adss/admin", Current);
                        FormHasError = !postResponse.IsSuccessStatusCode;

                        if (FormHasError)
                            FormError = postResponse.ReasonPhrase;
                        else
                        {
                            All.Add(Current);
                            Items.Add(Current);
                            Current = new Abeer.Shared.Functional.AdModel();
                            ModalFormAdVisible = false;
                        }

                        break;
                    }
                case "Edit":
                    {
                        var putResponse = await HttpClient.PutAsJsonAsync<Abeer.Shared.Functional.AdModel>("/api/adss/admin", Current);
                        FormHasError = !putResponse.IsSuccessStatusCode;

                        if (FormHasError)
                        {
                            FormError = putResponse.ReasonPhrase;
                        }
                        else
                        {
                            Current = new Abeer.Shared.Functional.AdModel();
                            ModalFormAdVisible = false;
                        }
                        break;
                    }
                case "Delete":
                    {
                        var deleteResponse = await HttpClient.DeleteAsync($"/api/adss/{Current.Id}");
                        FormHasError = !deleteResponse.IsSuccessStatusCode;

                        if (FormHasError)
                        {
                            FormError = deleteResponse.ReasonPhrase;
                        }
                        else
                        {
                            Current = new Abeer.Shared.Functional.AdModel();
                            Items.Remove(Current);
                            ModalFormAdVisible = false;
                        }
                        break;
                    }
            }

            await InvokeAsync(StateHasChanged);
        }
        bool IsFormDisabled => Mode == "Delete";
    }
}
