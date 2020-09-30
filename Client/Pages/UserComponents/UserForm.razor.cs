using Abeer.Shared;
using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using System;
using System.Net.Http;

namespace Abeer.Client.Pages
{
    public partial class UserForm : ComponentBase
    {
        [Parameter]
        public EventCallback<ApplicationUser> AddSuggestedUser { get; set; }
        [Inject] protected HttpClient HttpClient { get; set; }

        [Parameter]
        public ApplicationUser User { get; set; }
        [Parameter]
        public string Mode { get; set; }

        protected override void OnParametersSet()
        {
            if (User != null)
                Console.WriteLine($"User:{JsonConvert.SerializeObject(User)}");

            base.OnParametersSet();
        }
    }
}
