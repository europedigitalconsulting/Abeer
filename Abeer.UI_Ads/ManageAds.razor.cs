using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace Abeer.UI_Ads
{
    public partial class ManageAds : ComponentBase
    {
        private bool ModalFormAdVisible;
        private bool FormHasError;

        private string TitleForm = "EditForm";
        private string Mode = "Insert";

        private Abeer.Shared.Functional.AdModel Current = new Abeer.Shared.Functional.AdModel();

        public string Term { get; set; }

        private List<Abeer.Shared.Functional.AdModel> All = new List<Abeer.Shared.Functional.AdModel>();
        private List<Abeer.Shared.Functional.AdModel> Items = new List<Abeer.Shared.Functional.AdModel>();

        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject] private NavigationManager navigationManager { get; set; }

        private void countTerm(KeyboardEventArgs e)
        {
            if (Term?.Length > 5)
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

            var getAll = await HttpClient.GetAsync("/api/Ads/admin");
            if (getAll.IsSuccessStatusCode)
            { 
                var json = await getAll.Content.ReadAsStringAsync();
                All = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Abeer.Shared.Functional.AdModel>>(json);
                if (All != null)
                {
                    Items = All.ToList();
                }
            } 
        }

        private void Search()
        { 
            if (All != null && All.Count > 0)
                Items = All.Where(a => (a.Title != null && a.Title.Contains(Term)) || (a.Description != null && a.Description.Contains(Term))).ToList();
        }

        private void OpenEditModal(Abeer.Shared.Functional.AdModel adModel)
        {
            Current = adModel;
            Mode = "Edit";
            ModalFormAdVisible = true;
        }

        private void OpenDeleteModal(Abeer.Shared.Functional.AdModel adModel)
        {
            Current = adModel;
            Mode = "Delete";
            ModalFormAdVisible = true;
        }

        private void OpenCreateAd()
        {
            Current = new Abeer.Shared.Functional.AdModel();
            Current.StartDisplayTime = DateTime.UtcNow;
            Mode = "Insert";
            ModalFormAdVisible = true;
        }

        private string FormError = "";

        private async Task Save()
        {
            FormHasError = false;
            FormError = "";

            switch (Mode)
            {
                case "Insert":
                    {
                        var postResponse = await HttpClient.PostAsJsonAsync<Abeer.Shared.Functional.AdModel>("/api/Ads/admin", Current);
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
                        var putResponse = await HttpClient.PutAsJsonAsync<Abeer.Shared.Functional.AdModel>("/api/Ads/admin", Current);
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
                        var deleteResponse = await HttpClient.DeleteAsync($"/api/Ads/{Current.Id}");
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

        private bool IsFormDisabled => Mode == "Delete";
    }
}
