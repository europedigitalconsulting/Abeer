using Abeer.Shared;

using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace Abeer.Client
{
    public partial class CardForm : ComponentBase
    {
        [Parameter]
        public Batch Card { get; set; }

        [Parameter]
        public string Mode { get; set; }

        [Parameter]
        public IEnumerable<string> CardTypes { get; set; }

        protected override void OnParametersSet()
        {
            if (Card != null)
                Console.WriteLine($"User:{JsonConvert.SerializeObject(Card)}");

            base.OnParametersSet();
        }
    }
}
