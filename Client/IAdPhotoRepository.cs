using Microsoft.AspNetCore.Components;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abeer.Client
{
    public interface IAdPhotoRepository
    {
        Task<string> UploadAdImage(MultipartFormDataContent content, HttpClient httpClient, NavigationManager navigationManager);
    }

    public class AdHttpPhotoRepository : IAdPhotoRepository
    {
        public async Task<string> UploadAdImage(MultipartFormDataContent content, HttpClient httpClient, NavigationManager navigationManager)
        {
            var postResult = await httpClient.PostAsync("api/upload", content);
            
            var postContent = await postResult.Content.ReadAsStringAsync();
            
            if (!postResult.IsSuccessStatusCode)
            {
                throw new ApplicationException(postContent);
            }
            else
            {
                var imgUrl = navigationManager.ToAbsoluteUri(postContent).ToString();
                return imgUrl;
            }
        }
    }
}
