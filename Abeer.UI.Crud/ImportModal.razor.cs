using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Abeer.Shared.ViewModels;
using Tewr.Blazor.FileReader;

namespace Abeer.UI.Crud
{
    public partial class ImportModal
    {
        private ElementReference _input;
        private bool _isModalVisible;
        private string _fileName;
        private ImportOptionsViewModel _options;
        private byte[] _content;

        [Parameter]
        public IStringLocalizer StringLocalizer { get; set; }

        [Parameter]
        public string Id { get; set; }

        [Inject]
        public IFileReaderService FileReaderService { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        [Parameter]
        public string ApiPostFileUrl { get; set; }
        public EventCallback FilePosted { get; set; }

        public ImportModal()
        {
            Id = Guid.NewGuid().ToString();
            _options = new ImportOptionsViewModel();
        }

        private async Task HandleSelected()
        {
            foreach (var file in await FileReaderService.CreateReference(_input).EnumerateFilesAsync())
            {
                if (file != null)
                {
                    var fileInfo = await file.ReadFileInfoAsync();
                    using var ms = await file.CreateMemoryStreamAsync(4 * 1024);
                    _fileName = fileInfo.Name;
                    _options.FileName = _fileName;
                    _content = ms.ToArray();
                    ms.Close();
                    break;
                }
            }
        }

        public bool HasHeader { get; set; }
        public int SkipFirstRows { get; set; }
        public int LimitRows { get; set; }
        public bool Reset { get; set; }

        async Task Import()
        {
            var options = new ImportOptionsViewModel
            { 
                HasHeader = HasHeader,
                Reset = Reset,
                SkipFirstRows = SkipFirstRows, 
                LimitRows = LimitRows ,
                Data = _content, 
                FileName = _options.FileName
            };

            var postResponse = await HttpClient.PostAsJsonAsync(ApiPostFileUrl, options);
            postResponse.EnsureSuccessStatusCode();
        }

        public async Task ShowDialog()
        {
            _isModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
    }
}