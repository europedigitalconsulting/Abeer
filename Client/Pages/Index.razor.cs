using Abeer.Shared;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class Index
    {
        public IEnumerable<string> Months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
        public string CurrentMonth => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);

        public IEnumerable<Article> Articles { get; set; }

        ClaimsPrincipal User;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            var authState = await authenticationStateTask;

            User = authState.User;
            await InvokeAsync(StateHasChanged);
        }

        /*
        protected WalletStatistics WalletStatistics { get; set; }
        protected MarketStatistics MarketStatistics { get; set; }
        */

        public string MonthSelectedClass(string month)
        {
            if (System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month).Equals(month, StringComparison.OrdinalIgnoreCase))
                return "active";

            return "";
        }
    }
}
