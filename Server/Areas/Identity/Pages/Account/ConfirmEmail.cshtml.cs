using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Abeer.Shared;
using Abeer.Services;
using System;

namespace Abeer.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;
        private readonly EventTrackingService _eventTrackingService;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager, NotificationService notificationService, EventTrackingService eventTrackingService)
        {
            _userManager = userManager;
            _notificationService = notificationService;
            _eventTrackingService = eventTrackingService;
        }

        public bool Success { get; set; }
        public string ReturnUrl { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code, string returnUrl)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            ReturnUrl = returnUrl;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            Success = result.Succeeded;

            if (Success)
            {
                await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow,
                    Category = "profile",
                    Key = "confirmEmail",
                    UserId = user.Id
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            return await Task.Run(() =>
            {
                returnUrl ??= Url.Content("~/");
                return LocalRedirect(returnUrl);
            });
        }
    }
}
