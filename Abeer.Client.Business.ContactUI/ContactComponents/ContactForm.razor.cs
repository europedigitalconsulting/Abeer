using Abeer.Shared;

using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class ContactForm : ComponentBase
    {
        protected string contactType = "ById";
        protected bool Facebook;
        protected bool WhatsApp;
        protected bool Twitter;
        protected bool LinkedIn;
        protected bool Skype;
        protected bool ShowContactSuggestion;
        protected List<Contact> Suggestions = new List<Contact>();
        [Parameter]
        public EventCallback<Contact> AddSuggestedContact { get; set; }
        [Inject] protected HttpClient HttpClient { get; set; }

        [Parameter]
        public Contact Contact { get; set; }
        [Parameter]
        public string Mode { get; set; }

        protected override void OnParametersSet()
        {
            if (!string.IsNullOrWhiteSpace(Contact?.DisplayName))
                contactType = "ByName";
            else
                contactType = "ById";

            Facebook = !string.IsNullOrWhiteSpace(Contact?.FacebookUrl);
            WhatsApp = !string.IsNullOrWhiteSpace(Contact?.WhatsAppUrl);
            Twitter = !string.IsNullOrWhiteSpace(Contact?.TwitterUrl);
            LinkedIn = !string.IsNullOrWhiteSpace(Contact?.LinkedInUrl);
            Skype = !string.IsNullOrWhiteSpace(Contact?.SkypeUrl);
        }

        protected async Task FillSuggestion()
        {
            if(Contact.UserId.Length > 3)
            {
                var response = await HttpClient.GetAsync($"/api/Contacts/Suggestion?Term={Contact.UserId}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"suggestion result {json}");
                Suggestions = JsonConvert.DeserializeObject<List<Contact>>(json);
                StateHasChanged();
            }
        }

        protected async Task AddContact(Contact contact)
        {
            if (AddSuggestedContact.HasDelegate)
                await AddSuggestedContact.InvokeAsync(contact);
            else
                Console.WriteLine("no delegate found to AddSuggestedContact");
        }     
    }
}
