using Abeer.Ads.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Abeer.UI_Ads.Components
{
    public partial class FormAd
    {
        [Parameter]
        public Microsoft.Extensions.Localization.IStringLocalizer Loc { get; set; }
        [Parameter]
        public AdViewModel Ad { get; set; }
        [Parameter]
        public EventCallback Cancel { get; set; }       
        [Parameter]
        public bool FormHasError { get; set; }
        [Parameter]
        public string FormError { get; set; }
        [Parameter]
        public bool Disabled { get; set; }
        public bool ShowCategory { get; set; } = false;
        public bool ShowFamily { get; set; } = false;
        public string FamilySearch { get; set; }
        public AdsFamilyViewModel FamilySelected { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }
        protected List<AdsFamilyViewModel> ListFamily { get; set; } = new List<AdsFamilyViewModel>();
        protected List<AdsFamilyViewModel> ListFamilyTmp { get; set; } = new List<AdsFamilyViewModel>();
        protected List<AdsCategoryViewModel> ListCateg { get; set; } = new List<AdsCategoryViewModel>();
        private void AssignImageUrl1(string imgUrl) => Ad.ImageUrl1 = imgUrl;
        private void AssignImageUrl2(string imgUrl) => Ad.ImageUrl2 = imgUrl;
        private void AssignImageUrl3(string imgUrl) => Ad.ImageUrl3 = imgUrl;
        private void AssignImageUrl4(string imgUrl) => Ad.ImageUrl4 = imgUrl;

        protected override async Task OnInitializedAsync()
        {
            var getFamily = await HttpClient.GetAsync($"/api/bo/Families/FamiliesBy/{Ad.Id}");
            getFamily.EnsureSuccessStatusCode();
            var json = await getFamily.Content.ReadAsStringAsync();
            ListFamilyTmp = ListFamily = JsonConvert.DeserializeObject<List<AdsFamilyViewModel>>(json);
            FamilySelected = ListFamilyTmp.FirstOrDefault(x => x.Categories.Any(c => c.Selected));
            if (FamilySelected != null)
            {
                FamilySearch = FamilySelected.Label;
                ListCateg = FamilySelected.Categories;
                ShowCategory = true;
            }
            await InvokeAsync(StateHasChanged);
            await base.OnInitializedAsync();
        }
        private void CheckboxChanged(ChangeEventArgs e, string key)
        {
            var i = ListCateg.FirstOrDefault(i => i.Code == key);
            if (i != null)
            {
                i.Selected = (bool)e.Value;
            }
        }
        private void SelectFamily(AdsFamilyViewModel family)
        {
            family.Categories.ForEach(x => x.Selected = false); 
            FamilySelected = family;
            FamilySearch = FamilySelected.Label;
            ListCateg = family.Categories;
            ShowFamily = false;
            ShowCategory = true;
            StateHasChanged();
        }
        void TapSearch(ChangeEventArgs e)
        {
            FamilySearch = e.Value.ToString();
            ListFamily = ListFamilyTmp.Where(x => x.Label.StartsWith(FamilySearch, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        void FocusSearchFamily()
        {
            ShowFamily = !ShowFamily; 
            ShowCategory = false;
        }
        //private void ValiderCategories()
        //{
        //    Console.WriteLine(JsonConvert.SerializeObject(ListCateg));
        //    ShowCategory = false;
        //    StateHasChanged();
        //}
        private async Task Update()
        {
            Ad.ListIdCategory = ListCateg.Where(c => c.Selected).Select(x => x.CategoryId).ToList();
            var update = await HttpClient.PutAsJsonAsync<AdViewModel>("/api/Ads", Ad);
            update.EnsureSuccessStatusCode();
            await Close();
            await InvokeAsync(StateHasChanged);
        }
        private async Task Close()
        {
            await Cancel.InvokeAsync(); 
        }
    }
}
