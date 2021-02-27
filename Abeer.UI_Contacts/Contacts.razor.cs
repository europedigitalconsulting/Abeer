using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Abeer.Shared;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Abeer.UI_Contacts
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
        public string TermMyContacts { get; set; } = "";
        public bool ShowContactAddModal { get; set; }

        private async Task SearchAll()
        {
            await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(TermMyContacts))
                    Items = All.ToList();
                else
                    Items = All.Where(c => c.FirstName.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase)
                        || c.LastName.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase)
                        || c.Description.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase) ||
                        c.DisplayName.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase) ||
                        c.Email.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase) ||
                        c.Title.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase)).ToList();
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

        private async Task Add(ViewContact contact)
        {
            var response = await HttpClient.GetAsync($"/api/Contacts/add/{contact.UserId}");

            if (response.IsSuccessStatusCode)
            { 
                SuggestionItems.Remove(contact);
            } 
            await InvokeAsync(StateHasChanged);
        }

        async Task Remove(ViewContact contact)
        {
            var deleteResult =await HttpClient.DeleteAsync($"/api/contacts/{contact.Id}");
            deleteResult.EnsureSuccessStatusCode();
            Items.Remove(contact);
            await InvokeAsync(StateHasChanged);
        }

    }
}
