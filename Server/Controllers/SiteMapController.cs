using Abeer.Data.UnitOfworks;
using Abeer.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.Web.Sitemap;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteMapController : ControllerBase
    {
        private readonly FunctionalUnitOfWork functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;

        public SiteMapController(FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.functionalUnitOfWork = functionalUnitOfWork;
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [Produces("application/xml")]
        [HttpGet("")]
        public async Task<ActionResult<string>> GetSiteMapGlobal()
        {
            var users = await userManager.Users.Where(u => u.IsUnlimited || u.SubscriptionEndDate > DateTime.UtcNow).ToListAsync();

            var sitemap = new Sitemap
            {
                new X.Web.Sitemap.Url
                {
                    ChangeFrequency = ChangeFrequency.Hourly,
                    Location = $"{configuration["Service:FrontOffice:Url"].TrimEnd('/')}/api/rss",
                    Priority = 1,
                    LastMod = DateTime.UtcNow.Date.ToString("dd/MM/yyy")
                }
            };

            foreach (var user in users)
            {
                sitemap.Add(new X.Web.Sitemap.Url
                {
                    ChangeFrequency = ChangeFrequency.Daily,
                    Location = $"{configuration["Service:FrontOffice:Url"].TrimEnd('/')}/viewprofile/{(!string.IsNullOrEmpty(user.PinDigit) ? user.PinDigit : user.Id)}",
                    LastMod = DateTime.UtcNow.Date.ToString("dd/MM/yyy")
                });
            }

            var articles = await functionalUnitOfWork.AdRepository.GetVisibled();

            foreach (var article in articles)
            {
                sitemap.Add(new Url
                {
                    ChangeFrequency = ChangeFrequency.Daily,
                    Location = $"{configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ads/details/{article.Id}",
                    LastMod = DateTime.UtcNow.Date.ToString("dd/MM/yyy")
                });
            }

            return Content(sitemap.ToXml(), "application/xml");
        }

        [Produces("application/xml")]
        [HttpGet("{userId}")]
        public async Task<ActionResult<string>> GetSiteMapForUser(string userId)
        {
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

            var sitemap = new Sitemap
            {
                new X.Web.Sitemap.Url
                {
                    ChangeFrequency = ChangeFrequency.Hourly,
                    Location = $"{configuration["Service:FrontOffice:Url"].TrimEnd('/')}/api/rss/{user.Id}",
                    Priority = 1,
                    LastMod = DateTime.UtcNow.Date.ToString("dd/MM/yyy")
                }
            };

            var articles = await functionalUnitOfWork.AdRepository.GetVisibledUser(user.Id);

            foreach (var article in articles)
            {
                sitemap.Add(new Url
                {
                    ChangeFrequency = ChangeFrequency.Daily,
                    Location = $"{configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ads/details/{article.Id}",
                    LastMod = DateTime.UtcNow.Date.ToString("dd/MM/yyy")
                });
            }

            return Content(sitemap.ToXml(), "application/xml");
        }
    }
}
