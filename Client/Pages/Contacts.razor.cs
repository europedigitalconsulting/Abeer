
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
            var getAllContacts = await HttpClient.GetAsync("/api/Contacts/Suggestions");
            getAllContacts.EnsureSuccessStatusCode();

            var json = await getAllContacts.Content.ReadAsStringAsync();
            Suggestions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ViewContact>>(json);
            SuggestionItems = Suggestions;

            var getMyContacts = await HttpClient.GetAsync("/api/Contacts");
            getMyContacts.EnsureSuccessStatusCode();

            json = await getMyContacts.Content.ReadAsStringAsync();
            All = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ViewContact>>(json);

            Items = All.ToList();

            await base.OnParametersSetAsync();
        }

        public string Term { get; set; } = "";
        public bool ShowContactAddModal { get; set; }

        private void SearchAll()
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

            StateHasChanged();
        }

        private void Search()
        {
            if (string.IsNullOrWhiteSpace(Term))
                SuggestionItems = Suggestions.ToList();
            else
                SuggestionItems = Suggestions.Where(c => c.FirstName.Contains(Term, StringComparison.OrdinalIgnoreCase)
                                   || c.LastName.Contains(Term, StringComparison.OrdinalIgnoreCase)
                                   || c.Description.Contains(Term, StringComparison.OrdinalIgnoreCase) ||
                                   c.DisplayName.Contains(Term, StringComparison.OrdinalIgnoreCase) ||
                                   c.Email.Contains(Term, StringComparison.OrdinalIgnoreCase) ||
                                   c.Title.Contains(Term, StringComparison.OrdinalIgnoreCase)).ToList();

            StateHasChanged();
        }

        private void ToggleAddContact()
        {
            ShowContactAddModal = !ShowContactAddModal;
        }

        private void countSearchAll()
        {
            if (Term.Length >= 5)
                SearchAll();
        }
        private void countSearch()
        {
            if (Term.Length >= 5)
                Search();
        }

        private void Import(string id)
        {
            var getMyContacts = HttpClient.GetAsync($"/api/Contacts/import/{id}").GetAwaiter().GetResult();
            getMyContacts.EnsureSuccessStatusCode();
        }

    }
}
