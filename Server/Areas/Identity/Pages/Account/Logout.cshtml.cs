using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Abeer.Shared;
using Abeer.Services;
using System;

namespace Abeer.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly EventTrackingService _eventTrackingService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, EventTrackingService eventTrackingService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _eventTrackingService = eventTrackingService;
        }

        public async Task<IActionResult> OnGet(string returnUrl = null)
        {
            await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Category = "Logout",
                Key = "Get", 
                UserId = User.NameIdentifier()
            });

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Category = "Logout",
                Key = "Post",
                UserId = User.NameIdentifier()
            });

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage();
        }
    }
}
