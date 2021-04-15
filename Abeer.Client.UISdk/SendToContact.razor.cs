using Abeer.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Abeer.Client.UISdk
{
    public partial class SendToContact
    {
        [Parameter]
        public string TargetUrl { get; set; }
        [Parameter]
        public ViewApplicationUser User { get; set; }
        [Parameter]
        public Microsoft.Extensions.Localization.IStringLocalizer Loc { get; set; }
        string Subject { get; set; }
        string Body { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }

        [CascadingParameter]
        private Abeer.Shared.Functional.ScreenSize ScreenSize { get; set; }

        bool EditContact = true;
        string searchContactInput;
        bool NotFoundContact;
        bool DisplayListContact;
        bool Show;

        IList<ViewApplicationUser> ListContacts { get; set; } = new List<ViewApplicationUser>();
        ViewApplicationUser Contact { get; set; }

        string SearchContactInput
        {
            get => searchContactInput; set
            {
                searchContactInput = value;

                if (!string.IsNullOrEmpty(searchContactInput) && searchContactInput.Length >= 5)
                {
                    Task.Run(async () => await StartSearchContact());
                }
            }
        }

        async Task StartSearchContact()
        {
            var searchContactResult = await HttpClient.GetAsync($"api/contacts/find?term={SearchContactInput}");
            searchContactResult.EnsureSuccessStatusCode();
            ListContacts = JsonConvert.DeserializeObject<List<ViewApplicationUser>>(await searchContactResult.Content.ReadAsStringAsync());
            DisplayListContact = ListContacts?.Any() == true;
            NotFoundContact = !DisplayListContact;
        }

        void StartEditContact()
        {
            EditContact = true;
            NotFoundContact = false;
            SearchContactInput = Contact?.DisplayName;
        }

        void SetContact(ViewApplicationUser contact)
        {
            if (contact == null)
                return;

            Contact = contact;

            DisplayListContact = false;
            NotFoundContact = false;
            EditContact = false;
        }

        async Task SendProfile()
        {
            var response = await HttpClient.PostAsJsonAsync<SendProfileViewModel>("api/contacts/send", new SendProfileViewModel
            {
                Body = Body, Subject = Subject, UserId = User.Id, TargetUrl = TargetUrl, SendToId = Contact.Id
            });

            response.EnsureSuccessStatusCode();
            Show = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
