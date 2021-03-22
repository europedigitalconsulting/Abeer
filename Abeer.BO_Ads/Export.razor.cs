using Abeer.Ads.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Ads
{
    public partial class Export
    {
        private ExportOptionsViewModel _options = new ExportOptionsViewModel();
        private string _apiPostFileUrl = "api/bo/export";
        private ExportResultViewModel ExportResult;

        private bool ShowExportResultDialog;
        private bool FilterByFamily = false;
        private bool FilterByCategory = false;
        private bool FilterByProductType = false;

        private IList<AdsFamilyViewModel> Families { get; set; }
        private IList<AdsCategoryViewModel> Categories { get; set; } 

        [Inject]
        public HttpClient HttpClient { get; set; }

        [Inject]
        IJSRuntime ThisJSRuntime { get; set; }

        void SelectCategory(ChangeEventArgs e, AdsCategoryViewModel category)
        {
            bool isChecked = (bool)e.Value;

            if (isChecked)
            {
                if (string.IsNullOrEmpty(_options.Category))
                    _options.Category = category.CategoryId.ToString();
                else
                {
                    var items = new List<string>(_options.Category.Split(';'))
                    {
                        category.CategoryId.ToString()
                    };

                    _options.Category = string.Join(';', items);
                }
            }
            else if (!string.IsNullOrEmpty(_options.Category))
            {
                var list = new List<string>(_options.Category.Split(';'));
                list.Remove(category.CategoryId.ToString());
                _options.Category = string.Join(';', list);
            }
        }

        void SelectFamily(ChangeEventArgs e, AdsFamilyViewModel family)
        {
            bool isChecked = e.Value.ToString() == "on";

            if (isChecked)
            {
                if (string.IsNullOrEmpty(_options.Family))
                    _options.Family = family.FamilyId.ToString();
                else
                {
                    var items = new List<string>(_options.Family.Split(';'))
                    {
                        family.FamilyId.ToString()
                    };

                    _options.Family = string.Join(';', items);
                }
            }
            else if (!string.IsNullOrEmpty(_options.Family))
            {
                var list = new List<string>(_options.Family.Split(';'));
                list.Remove(family.FamilyId.ToString());
                _options.Family = string.Join(';', list);
            }
        }

        async Task DownloadDocument()
        {
            await ThisJSRuntime.InvokeVoidAsync(
                "downloadFromByteArray",
                new
                {
                    ByteArray = Convert.ToBase64String(ExportResult.FileContent),
                    FileName = $"data_{DateTime.UtcNow.ToString("yyyMMddHHmmss") + _options.FileType}",
                    ContentType = ExportResult.FileMimeType
                });
        }
         
        bool IsFamilySelected(AdsFamilyViewModel family)
        {
            if (_options == null)
                return false;

            if (string.IsNullOrEmpty(_options.Family))
                return false;

            var list = new List<string>(_options.Family.Split(';'));

            return list.Contains(family.FamilyId.ToString());
        }
         
        bool IsCategorySelected(AdsCategoryViewModel category)
        {
            if (_options == null)
                return false;

            if (string.IsNullOrEmpty(_options.Category))
                return false;

            var list = new List<string>(_options.Category.Split(';'));

            return list.Contains(category.CategoryId.ToString());
        }


        async Task ShowFilterByFamily()
        {
            if (FilterByFamily)
            {
                FilterByFamily = false;
                return;
            }

            var httpGetFamilies = await HttpClient.GetAsync("api/Ads/families");
            httpGetFamilies.EnsureSuccessStatusCode();
            var json = await httpGetFamilies.Content.ReadAsStringAsync();
            Families = JsonConvert.DeserializeObject<List<AdsFamilyViewModel>>(json);
            FilterByFamily = true;
            _options.IncludeFamily = true;
            await InvokeAsync(StateHasChanged);
        }

        async Task ShowFilterByCategory()
        {
            if (FilterByCategory)
            {
                FilterByCategory = false;
                return;
            }

            var httpGetCategories = await HttpClient.GetAsync($"api/Ads/categories/filter/{_options.Family}");
            httpGetCategories.EnsureSuccessStatusCode();
            var json = await httpGetCategories.Content.ReadAsStringAsync();
            Categories = JsonConvert.DeserializeObject<List<AdsCategoryViewModel>>(json);
            FilterByCategory = true;
            _options.IncludeCategory = true;
            await InvokeAsync(StateHasChanged);
        }
         
        async Task GenerateFile()
        {
            var postResponse = await HttpClient.PostAsJsonAsync(_apiPostFileUrl, _options);
            postResponse.EnsureSuccessStatusCode();
            var json = await postResponse.Content.ReadAsStringAsync();
            ExportResult = JsonConvert.DeserializeObject<ExportResultViewModel>(json);
            ShowExportResultDialog = true;
        }
    }
}
