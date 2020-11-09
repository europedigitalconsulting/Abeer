
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
    public partial class Contacts : ComponentBase
    {
        public List<ViewContact> All { get; set; } = new List<ViewContact>();
        public List<ViewContact> Items { get; set; } = new List<ViewContact>();
        public List<ViewContact> Suggestions { get; set; } = new List<ViewContact>();
        public List<ViewContact> SuggestionItems { get; set; } = new List<ViewContact>();

        protected override async Task OnParametersSetAsync()
        {
            var getMyContacts = await HttpClient.GetAsync("/api/Contacts");
            getMyContacts.EnsureSuccessStatusCode();

            var json = await getMyContacts.Content.ReadAsStringAsync();
            All = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ViewContact>>(json);

            Items = All.ToList();

            await base.OnParametersSetAsync();
        }

        public string Term { get; set; } = "";
        public bool ShowContactAddModal { get; set; }

        private async Task SearchAll()
        {
            await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(Term))
                    Items = All.ToList();
                else
                    Items = All.Where(c => c.FirstName.Contains(Term, StringComparison.OrdinalIgnoreCase)
                        || c.LastName.Contains(Term, StringComparison.OrdinalIgnoreCase)
                        || c.Description.Contains(Term, StringComparison.OrdinalIgnoreCase) ||
                        c.DisplayName.Contains(Term, StringComparison.OrdinalIgnoreCase) ||
                        c.Email.Contains(Term, StringComparison.OrdinalIgnoreCase) ||
                        c.Title.Contains(Term, StringComparison.OrdinalIgnoreCase)).ToList();
            });

            await InvokeAsync(StateHasChanged);
        }

        private async Task GetSuggestions()
        {
            if (!string.IsNullOrWhiteSpace(Term))
            {
                var getSuggestion = await HttpClient.GetAsync($"api/Contacts/suggestions?Term={Term}");
                getSuggestion.EnsureSuccessStatusCode();
                var json = await getSuggestion.Content.ReadAsStringAsync();
                SuggestionItems = JsonConvert.DeserializeObject<List<ViewContact>>(json);
            }

            await InvokeAsync(StateHasChanged);
        }

        private void ToggleAddContact()
        {
            ShowContactAddModal = !ShowContactAddModal;
        }

        private async Task CountSearchAll()
        {
            if (Term.Length >= 5)
                await SearchAll();
        }

        private async Task CountSuggestion()
        {
            if (Term.Length >= 5)
                await GetSuggestions();
        }

        private void Import(string id)
        {
            var getMyContacts = HttpClient.GetAsync($"/api/Contacts/import/{id}").GetAwaiter().GetResult();
            getMyContacts.EnsureSuccessStatusCode();
        }

    }
}
