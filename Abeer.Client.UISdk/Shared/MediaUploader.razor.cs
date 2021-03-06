using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Tewr.Blazor.FileReader;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;

namespace Abeer.Client.UISdk.Shared
{
    public partial class MediaUploader
    {
        private ElementReference _input;
        [Parameter]
        public string ImgUrl { get; set; }
        [Parameter]
        public EventCallback<string> OnChange { get; set; }
        [Inject]
        public IFileReaderService FileReaderService { get; set; }
        [Inject]
        public IAdPhotoRepository Repository { get; set; }
        [Inject]
        public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        private async Task HandleSelected(ChangeEventArgs changeEventArgs)
        {
            foreach (var file in await FileReaderService.CreateReference(_input).EnumerateFilesAsync())
            {
                if (file == null) continue;
                var fileInfo = await file.ReadFileInfoAsync();

                await using var ms = await file.CreateMemoryStreamAsync(4 * 1024);
                var content = new MultipartFormDataContent();
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                content.Add(new StreamContent(ms, Convert.ToInt32(ms.Length)), "image", fileInfo.Name);
                ImgUrl = await Repository.UploadAdImage(content, HttpClientFactory.CreateClient(Configuration["Service:Api:AnonymousApiName"]), 
                    NavigationManager);
                Console.WriteLine($"Upload Ad Image {ImgUrl}");
                await OnChange.InvokeAsync(ImgUrl);
            }
        }
    }
}
