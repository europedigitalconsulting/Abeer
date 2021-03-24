using Abeer.Data.UnitOfworks;
using Abeer.Shared;
using Abeer.Shared.Technical;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RssController : ControllerBase
    {
        private readonly FunctionalUnitOfWork functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;

        public RssController(FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.functionalUnitOfWork = functionalUnitOfWork;
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [Produces("application/xml")]
        [HttpGet("")]
        public async Task<ActionResult<string>> GetFeedsGlobal()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("<rss version=\"2.0\">");
            stringBuilder.AppendLine("<channel>");
            stringBuilder.AppendLine("<title>Meetag.co rss</title>");
            stringBuilder.AppendLine("<copyright>Meetag.co</copyright>");
            stringBuilder.AppendLine($"<link>{configuration["Service:FrontOffice:Url"]}</link> ");
            stringBuilder.AppendLine($"<language>{CultureInfo.CurrentCulture.TwoLetterISOLanguageName}</language>");
            stringBuilder.AppendLine("<description>meetag.co feeds</description>");

            var articles = await functionalUnitOfWork.AdRepository.GetVisibled();

            foreach (var article in articles)
            {
                var url = $"{configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ads/details/{article.Id}";
                stringBuilder.AppendLine("<item>");
                stringBuilder.AppendLine($"<title>{article.Title}</title>");

                var image = article.ImageUrl1 ?? article.ImageUrl2 ?? article.ImageUrl3 ?? article.ImageUrl4;

                if (!string.IsNullOrEmpty(image))
                {
                    stringBuilder.AppendLine("<image>");
                    stringBuilder.AppendLine($"<url>{image}</url>");
                    stringBuilder.AppendLine($"<title>{image}</title>");
                    stringBuilder.AppendLine($"<link>{image}</link>");
                    stringBuilder.AppendLine("</image>");
                }

                stringBuilder.AppendLine($"<link>{url}</link>");
                stringBuilder.AppendLine($"<description>{article.Description}</description>");

                var user = await userManager.FindByIdAsync(article.OwnerId);

                stringBuilder.AppendLine($"<author>{user.DisplayName}-{user.Email}</author>");

                stringBuilder.AppendLine($"</item>");
            }

            stringBuilder.AppendLine("</channel>");
            stringBuilder.AppendLine("</rss>");

            return Content(stringBuilder.ToString(), "application/xml");
        }

        [Produces("application/xml")]
        [HttpGet("{userId}")]
        public async Task<ActionResult<string>> GetSiteMapForUser(string userId)
        {
            var stringBuilder = new StringBuilder();

            var user = await userManager.FindByIdAsync(userId);

            if (!user.IsAdmin && !user.IsUnlimited)
            {
                var subscription = await functionalUnitOfWork.SubscriptionRepository.GetLatestSubscriptionForUser(user.Id);

                if (subscription != null && subscription.SubscriptionPackId != Guid.Empty)
                {
                    var pack = await functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(s => s.Id == subscription.SubscriptionPackId);

                    if (subscription != null)
                        if (pack.Label.ToLower() != "ultimate")
                            return BadRequest();
                }
            }

            if (user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value < DateTime.UtcNow)
                return BadRequest();

            stringBuilder.AppendLine("<rss version=\"2.0\">");
            stringBuilder.AppendLine("<channel>");
            stringBuilder.AppendLine("<title>Meetag.co rss</title>");
            stringBuilder.AppendLine("<copyright>Meetag.co</copyright>");
            stringBuilder.AppendLine($"<language>{CultureInfo.CurrentCulture.TwoLetterISOLanguageName}</language>");
            stringBuilder.AppendLine($"<link>{configuration["Service:FrontOffice:Url"]}</link> ");
            stringBuilder.AppendLine($"<description>meetag.co feeds for user {user.DisplayName}</description>");

            var articles = await functionalUnitOfWork.AdRepository.GetVisibledUser(userId);

            foreach (var article in articles)
            {
                var url = $"{configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ads/details/{article.Id}";
                stringBuilder.AppendLine("<item>");
                stringBuilder.AppendLine($"<title>{article.Title}</title>");
                stringBuilder.AppendLine($"<link>{url}</link>");
                stringBuilder.AppendLine($"<description>{article.Description}</description>");
                
                var image = article.ImageUrl1 ?? article.ImageUrl2 ?? article.ImageUrl3 ?? article.ImageUrl4;
                
                if (!string.IsNullOrEmpty(image))
                {
                    stringBuilder.AppendLine("<image>");
                    stringBuilder.AppendLine($"<url>{image}</url>");
                    stringBuilder.AppendLine($"<title>{image}</title>");
                    stringBuilder.AppendLine($"<link>{image}</link>");
                    stringBuilder.AppendLine("</image>");
                }

                stringBuilder.AppendLine($"<author>{user.DisplayName}-{user.Email}</author>");

                stringBuilder.AppendLine($"</item>");
            }

            stringBuilder.AppendLine("</channel>");
            stringBuilder.AppendLine("</rss>");

            return Content(stringBuilder.ToString(), "application/xml");
        }
    }
}
