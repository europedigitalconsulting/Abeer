using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.ClientHub;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Abeer.Shared.ViewModels;

namespace Abeer.UI_Contacts
{
    public partial class Contacts : ComponentBase
    {
        [CascadingParameter] private NotificationClient NotificationClient { get; set; }
        [Inject] private HttpClient httpClient { get; set; }
        public List<ViewContact> All { get; set; } = new List<ViewContact>();
        public List<ViewContact> Items { get; set; } = new List<ViewContact>();
        public List<ViewContact> Suggestions { get; set; } = new List<ViewContact>();
        public List<ViewContact> SuggestionItems { get; set; } = new List<ViewContact>();
        public List<Country> Countries { get; set; } = new List<Country>();
        [CascadingParameter]
        public ScreenSize ScreenSize { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var getMyContacts = await httpClient.GetAsync("/api/Contacts");
            var getCountries = await httpClient.GetAsync("/api/Countries");

            getMyContacts.EnsureSuccessStatusCode();
            getCountries.EnsureSuccessStatusCode();

            var json2 = await getMyContacts.Content.ReadAsStringAsync();
            var jsonCountry = await getCountries.Content.ReadAsStringAsync();

            All = JsonConvert.DeserializeObject<List<ViewContact>>(json2);
            Countries = JsonConvert.DeserializeObject<List<Country>>(jsonCountry);
            Items = All.ToList();

            await base.OnInitializedAsync();
        }

        public Country FilterSelected { get; set; }
        public string Term { get; set; } = "";
        public string TermMyContacts { get; set; } = "";
        public bool ShowContactAddModal { get; set; }
        public bool Showfilter { get; set; }
        public bool ShowfilterExt { get; set; }
        public bool IsRequestsDisplayed { get; private set; }

        private async Task SearchAll()
        { 
            await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(TermMyContacts))
                    Items = All.ToList();
                else
                    Items = All.Where(c => (FilterSelected == null || c.Contact.Country == FilterSelected.Name)
                        && (c.Contact.FirstName.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase)
                        || c.Contact.LastName.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase)
                        || c.Contact.Description.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase) ||
                        c.Contact.DisplayName.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase) ||
                        c.Contact.Email.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase) ||
                        c.Contact.Title.Contains(TermMyContacts, StringComparison.OrdinalIgnoreCase))).ToList();
            });
            Showfilter = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetSuggestions()
        {
            if (!string.IsNullOrWhiteSpace(Term))
            {
                var getSuggestion = await httpClient.GetAsync($"api/Contacts/suggestions?Term={Term}&Filter={FilterSelected?.Name ?? ""}");
                getSuggestion.EnsureSuccessStatusCode();
                var json = await getSuggestion.Content.ReadAsStringAsync();
                SuggestionItems = JsonConvert.DeserializeObject<List<ViewContact>>(json);
                ShowfilterExt = false;
            }

            await InvokeAsync(StateHasChanged);
        }

        private void ToggleAddContact()
        {
            ShowContactAddModal = !ShowContactAddModal;
            Showfilter = false;
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
            var response = await httpClient.GetAsync($"/api/Contacts/add/{contact.Contact.Id}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var contactViewModel = JsonConvert.DeserializeObject<ContactViewModel>(json);
            contact.UserAccepted = Shared.ReferentielTable.EnumUserAccepted.PENDING;
            await InvokeAsync(StateHasChanged);

            await NotificationClient.SendNotificationsToUser(contactViewModel.Notification, contact.UserId);
        }

        async Task Remove(ViewContact contact)
        {
            var deleteResult = await httpClient.DeleteAsync($"/api/contacts/{contact.Id}");
            deleteResult.EnsureSuccessStatusCode();
            Items.Remove(contact);
            await InvokeAsync(StateHasChanged);
        }
        public async Task Enter(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await GetSuggestions();
                Showfilter = false;
            }
        }
        public async Task SelectFilter(Country item)
        {
            FilterSelected = item;
            ShowfilterExt = Showfilter = false;
            await InvokeAsync(StateHasChanged);
        }

        public async Task ToggleDisplayRequests()
        {
            HttpResponseMessage getItems;

            if (!IsRequestsDisplayed)
            {
                getItems = await httpClient.GetAsync("/api/Contacts/requests");
                IsRequestsDisplayed = true;
            }
            else
            {
                getItems = await httpClient.GetAsync("/api/Contacts");
                IsRequestsDisplayed = false;
            }

            getItems.EnsureSuccessStatusCode();
            var json = await getItems.Content.ReadAsStringAsync();
            All = JsonConvert.DeserializeObject<List<ViewContact>>(json);
            
            await SearchAll();
        }
    }
}
