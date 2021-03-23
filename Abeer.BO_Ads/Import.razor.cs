using Abeer.Ads.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tewr.Blazor.FileReader;

namespace Abeer.Ads
{
    public partial class Import
    {
        private ElementReference _input;
        private bool _isModalVisible;

        private ImportOptionsViewModel _options = new ImportOptionsViewModel();
        private string _apiPostFileUrl = "api/bo/Import";

        [Inject]
        public IFileReaderService FileReaderService { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        AdsImportResultViewModel ImportResult { get; set; }
        bool ShowImportResultDialog { get; set; }

        private async Task HandleSelected()
        {
            foreach (var file in await FileReaderService.CreateReference(_input).EnumerateFilesAsync())
            {
                if (file != null)
                {
                    var fileInfo = await file.ReadFileInfoAsync();
                    using var ms = await file.CreateMemoryStreamAsync(4 * 1024);
                    _options.FileName = fileInfo.Name;
                    _options.Data = ms.ToArray();
                    ms.Close();
                    break;
                }
            }
        }

        async Task ImportFile()
        {
            var postResponse = await HttpClient.PostAsJsonAsync(_apiPostFileUrl, _options);
            postResponse.EnsureSuccessStatusCode();
            var json = await postResponse.Content.ReadAsStringAsync();
            ImportResult = JsonConvert.DeserializeObject<AdsImportResultViewModel>(json);
            Console.WriteLine(ImportResult);
            ShowImportResultDialog = true;
            await InvokeAsync(StateHasChanged);
        }
    }
}
