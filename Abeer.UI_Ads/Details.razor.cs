﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Abeer.UI_Ads
{
    public partial class Details : ComponentBase
    {
        [Parameter]
        public  string Id { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }

        private AdViewModel Ad { get; set; }

        public int CurrentImageIndex { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            var getDetail = await HttpClient.GetAsync($"/api/ads/{Id}");
            getDetail.EnsureSuccessStatusCode();
            var json = await getDetail.Content.ReadAsStringAsync();
            Ad = JsonConvert.DeserializeObject<AdViewModel>(json);
            await base.OnInitializedAsync();
        }
    }
}
