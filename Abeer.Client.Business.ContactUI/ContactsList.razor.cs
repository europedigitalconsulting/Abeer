using Abeer.Shared;

using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class ContactsList : ComponentBase
    {
        bool showModal = false;

        public ContactForm ContactForm { get; set; }
        public string TitleDialog { get; set; }
        public List<Contact> Contacts { get; set; } = new List<Contact>();
        protected string SearchTerm { get; set; }
        protected List<Contact> Items = new List<Contact>();

        [Parameter]
        public string LabelCancel { get; set; }
        [Parameter]
        public string LabelDeleteTitle { get; set; }
        [Parameter]
        public string LabelDelete { get; set; }
        [Parameter]
        public string LabelInsertTitle { get; set; }
        [Parameter]
        public string LabelInsert { get; set; }
        [Parameter]
        public string LabelUpdateTitle { get; set; }
        [Parameter]
        public string LabelUpdate { get; set; }
        [Parameter]
        public string LabelConfirmDelete { get; set; }
        [Parameter]
        public string LabelHome { get; set; }
        [Parameter]
        public string LabelMyContactList { get; set; }
        [Parameter]
        public string LabelMyContactListCatchPhrase { get; set; }
        [Parameter]
        public string LabelSearch { get; set; }
        [Parameter]
        public string LabelAddContact { get; set; }
        [Parameter]
        public string LabelSendTo { get; set; }
        [Parameter]
        public string LabelNoContactFound { get; set; }
        [Parameter]
        public string ById { get; set; }
        [Parameter]
        public string ByName { get; set; }
        [Parameter]
        public string LabelContactId { get; set; }
        [Parameter]
        public string Search { get; set; }
        [Parameter]
        public string LabelDisplayName { get; set; }
        [Parameter]
        public string LabelEmail { get; set; }
        [Parameter]
        public string SocialLinks { get; set; }
        [Parameter]
        public string LabelCity { get; set; }
        [Parameter]
        public string LabelCountry { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("get /api/contacts");

            var response = await HttpClient.GetAsync("/api/contacts", HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Contacts = JsonConvert.DeserializeObject<List<Contact>>(json);
            Items = Contacts.ToList();
            await base.OnInitializedAsync();
        }

        public void SearchButtonClick()
        {
            Console.WriteLine($"start search contact {SearchTerm}");
            var found = Contacts?.Where(c => c.DisplayName?.Contains(SearchTerm) == true || c.Email?.Contains(SearchTerm) == true || c.UserId?.Contains(SearchTerm) == true)?.ToList();
            Items = found;
            Console.WriteLine($"{found.Count} contacts found");
            StateHasChanged();
        }
        void ModalClose() => showModal = false;
        void ModalShow() => showModal = true;
        void ModalCancel() => showModal = false;
        string Mode { get; set; } = "Insert";

        Contact current = null;

        void ShowInsertContact()
        {
            current = new Contact();
            Mode = "Insert";
            ModalShow();
            StateHasChanged();
        }
        void ShowEditContact(Contact contact)
        {
            current = contact;
            Mode = "Update";
            ModalShow();
            StateHasChanged();
        }

        void ShowDeleteContact(Contact contact)
        {
            current = contact;
            Mode = "Delete";
            ModalShow();
        }
        async Task Insert()
        {
            var response = await HttpClient.PostAsJsonAsync<Contact>("api/Contacts", current);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"insert result : {json}");
            var contact = JsonConvert.DeserializeObject<Contact>(json);
            Contacts.Add(contact);
            Items.Add(contact);
            ModalClose();
            StateHasChanged();
        }

        async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync<Contact>($"api/Contacts/{current.Id}", current);
            response.EnsureSuccessStatusCode();
            ModalClose();
            StateHasChanged();
        }

        async Task Delete()
        {
            var response = await HttpClient.DeleteAsync($"api/contacts/{current.Id}");
            response.EnsureSuccessStatusCode();
            Contacts.Remove(current);
            Items.Remove(current);
            ModalClose();
            StateHasChanged();
        }

        async Task AddSuggestedContact(Contact contact)
        {
            var response = await HttpClient.PostAsJsonAsync<Contact>("api/Contacts", contact);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"insert result : {json}");
            current = JsonConvert.DeserializeObject<Contact>(json);
            Contacts.Add(current);
            ModalClose();
            StateHasChanged();
        }
    }
}
