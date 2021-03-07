using Abeer.Data;
using Abeer.Data.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;

using System;
using System.Threading.Tasks;

namespace Abeer.Services
{
    public class UrlShortner
    {
        public UrlShortner(FunctionalDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public FunctionalDbContext DbContext { get; }

        public async Task<string> CreateUrl(bool isSecure, bool isSingleClick, string shortnedUrl, string longUrl , string secureKey, string code = "")
        {
            code = string.IsNullOrWhiteSpace(code) ? GenerateCode() : code;

            await Task.Run(() => DbContext.UrlShortneds.Add(new UrlShortned
            {
                Code = code,
                IsSecure = isSecure,
                IsSingle = isSingleClick,
                SecureKey = secureKey,
                LongUrl = longUrl,
                ShortUrl = shortnedUrl
            }));

            return shortnedUrl;
        }

        public async Task<string> CreateUrl(string scheme, HostString host, string longUrl, string code=null, bool isSingleClick=false, bool isSecure=false, string secureKey=null)
        {
            code = string.IsNullOrWhiteSpace(code) ? GenerateCode() : code;

            var shortedUrl = UriHelper.BuildAbsolute(scheme, host, $"/shortned/{code}");

            return await CreateUrl(isSecure, isSingleClick, shortedUrl, longUrl, secureKey, code);
        }

        public async Task<UrlShortned> Resolve(HttpContext context)
        {
            var httpRequest = context.Request;
            
            var shortUrl = UriHelper.BuildAbsolute(httpRequest.Scheme, httpRequest.Host, httpRequest.Path.Value);
            var shortned = await Task.Run(() => DbContext.UrlShortneds.FirstOrDefault(u => u.ShortUrl == shortUrl));

            if (shortned.IsSingle && shortned.IsClicked)
                throw new UnauthorizedAccessException();

            if(shortned.IsSecure && !context.Request.Query.ContainsKey("SecureCode"))
                throw new UnauthorizedAccessException();

            shortned.IsClicked = true;
            shortned.ClickedDate = DateTime.UtcNow;

            return shortned;
        }

        private static string GenerateCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}
