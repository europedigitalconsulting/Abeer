using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abeer.Data.UnitOfworks;
using Abeer.Shared;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Abeer.Server.Pages
{
    public class _HostModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly FunctionalUnitOfWork functionalUnitOfWork;

        public string PageType { get; set; }
        public string PageTitle { get; set; }
        public string FullUrl { get; set; }
        public string PageImage { get; set; }
        public string SiteName { get; set; }
        public string PageDescription { get; set; }
        public string CreateBy { get; set; }
        public bool IsPageIndexed { get; set; }

        public _HostModel(UserManager<ApplicationUser> userManager, FunctionalUnitOfWork functionalUnitOfWork)
        {
            this.userManager = userManager;
            this.functionalUnitOfWork = functionalUnitOfWork;
        }

        public async Task OnGetAsync()
        {
            SiteName = "meetag.co";

            if (Request.Path.Value.Contains("/ViewProfile/", StringComparison.OrdinalIgnoreCase))
            {
                string userId = Request.Path.Value.Substring(Request.Path.Value.LastIndexOf('/')).TrimStart('/');
                
                PageType = "profile";
                
                var profile = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId || u.PinDigit == userId);
                PageTitle = $"::Meetag.co::{profile.DisplayName} !!";
                FullUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, Request.PathBase, Request.Path);
                
                if (!string.IsNullOrEmpty(profile.PhotoUrl))
                    PageImage = profile.PhotoUrl;

                PageDescription = !string.IsNullOrEmpty(profile.Description) ? profile.Description : $"meetag.co :: User Profile {profile.DisplayName}";
                CreateBy = "meetag.co";
                IsPageIndexed = true;
            }
            else if(Request.Path.Value.Contains("/ads/details/", StringComparison.OrdinalIgnoreCase))
            {
                Guid articleId = Guid.Parse(Request.Path.Value.Substring(Request.Path.Value.LastIndexOf('/')).TrimStart('/'));
                PageType = "article";
                var article = await functionalUnitOfWork.AdRepository.FirstOrDefault(a => a.Id == articleId);
                PageTitle = $"::Meetag.co::{article.Title} !!";
                FullUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, Request.PathBase, Request.Path);
                
                if (!string.IsNullOrEmpty(article.ImageUrl1))
                    PageImage = article.ImageUrl1;

                if (!string.IsNullOrEmpty(article.ImageUrl2))
                    PageImage = article.ImageUrl2;

                if (!string.IsNullOrEmpty(article.ImageUrl3))
                    PageImage = article.ImageUrl3;

                if (!string.IsNullOrEmpty(article.ImageUrl4))
                    PageImage = article.ImageUrl4;

                PageDescription = !string.IsNullOrEmpty(article.Description) ? $"meetag.co :: " + article.Description : $"meetag.co :: {article.Title}";

                var user = await userManager.FindByIdAsync(article.OwnerId);

                CreateBy = $"meetag.co::{user.DisplayName}";
                IsPageIndexed = true;
            }
        }
    }
}
