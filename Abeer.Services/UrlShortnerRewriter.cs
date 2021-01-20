using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Abeer.Services
{
    public class UrlShortnerRewriter
    {
        private readonly RequestDelegate _next;

        public UrlShortnerRewriter(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UrlShortner urlShortner)
        {
            if (context.Request.Path.StartsWithSegments(new PathString("/shortned")))
            {
                var urlShortned = await urlShortner.Resolve(context);

                if (urlShortned != null)
                {
                    context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
                    context.Response.Headers[HeaderNames.Location] = urlShortned.LongUrl;
                    return;
                }
            }

            await _next.Invoke(context);
        }
    }
}
