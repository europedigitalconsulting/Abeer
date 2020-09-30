using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class MyAccount : ComponentBase
    {
        public string CurrentMonth => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
        public IEnumerable<string> Months => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;

        string SearchTerm { get; set; }

        async Task SearchButtonClick()
        {

        }
    }
}
