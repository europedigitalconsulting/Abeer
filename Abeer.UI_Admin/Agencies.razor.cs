using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Abeer.UI_Admin
{
    public partial class Agencies : ComponentBase
    {
        public List<Agency> All { get; set; } = new List<Agency>();
        public List<Agency> Items { get; set; } = new List<Agency>();
        public List<Agency> Suggestions { get; set; } = new List<Agency>();
        public List<Agency> SuggestionItems { get; set; } = new List<Agency>();
        [Inject] public IConfiguration Configuration { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var httpClient = HttpClientFactory.CreateClient(Configuration["Service:Api:AnonymousApiName"]);

            var getMyAgencies = await httpClient.GetAsync("/api/Agencies");
            getMyAgencies.EnsureSuccessStatusCode();

            var json = await getMyAgencies.Content.ReadAsStringAsync();
            All = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Agency>>(json);

            Items = All.ToList();

            await base.OnParametersSetAsync();
        }

        public string Term { get; set; } = "";
        public bool ShowAgencyAddModal { get; set; }

        private void Search()
        {
            if (string.IsNullOrWhiteSpace(Term))
                SuggestionItems = Suggestions.ToList();
            else
                SuggestionItems = Suggestions.Where(c => c.AgencyName.Contains(Term, StringComparison.OrdinalIgnoreCase)
                                   || c.DisplayName.Contains(Term, StringComparison.OrdinalIgnoreCase) ||
                                   c.Email.Contains(Term, StringComparison.OrdinalIgnoreCase)).ToList();

            StateHasChanged();
        }

        private void countSearch()
        {
            if (Term.Length >= 5)
                Search();
        }
    }
}
