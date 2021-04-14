using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared;
using Abeer.Shared.Security;
using IdentityModel;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWithPinCodeModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;
        private readonly string _webRoot;
        private readonly EventTrackingService _eventTrackingService;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;

        public LoginWithPinCodeModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IWebHostEnvironment env,
            IEmailSender emailSender,
            IConfiguration configuration, EventTrackingService eventTrackingService, 
            FunctionalUnitOfWork functionalUnitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            Configuration = configuration;
            _env = env;
            _webRoot = _env.WebRootPath;
            _eventTrackingService = eventTrackingService;
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public IConfiguration Configuration { get; }

        public class InputModel
        {
            [BindProperty(Name = "PinDigit", SupportsGet = true)]
            [Display(Name = "Pin Digit")]
            public string PinDigit { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Pin Code")]
            public int PinCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (Input == null)
                Input = new InputModel();

            if (!string.IsNullOrEmpty(Request.Query?["PinDigit"]))
            {
                Input.PinDigit = Request.Query?["PinDigit"];
            }

            await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Category = "LoginWithPinCode",
                Key = "Start"
            });


            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Profile");

            if (ModelState.IsValid)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PinCode == Input.PinCode && u.PinDigit ==  Input.PinDigit );

                if (user == null)
                {
                    ModelState.AddModelError("", "User is not authorized");
                    return Page();
                }

                await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow,
                    Category = "Login",
                    Key = "PinCode", 
                    UserId = user.Id
                });


                user.IsOnline = true;
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var identity = new ClaimsIdentity("Identity.Application");

                user.WriteClaims(identity);

                if (!user.IsAdmin && !user.IsUnlimited)
                {
                    var subscription = await _functionalUnitOfWork.SubscriptionRepository.GetLatestSubscriptionForUser(user.Id);

                    if (subscription != null && subscription.SubscriptionPackId != Guid.Empty)
                    {
                        var pack = await _functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(s => s.Id == subscription.SubscriptionPackId);

                        if (subscription != null)
                            identity.AddClaim(ClaimNames.Subscription, pack.Label.ToLower());
                    }
                }

                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("Identity.Application", principal);

                return LocalRedirect(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private string GetDisplayName(ApplicationUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.DisplayName))
                return user.DisplayName;
            else if (!string.IsNullOrWhiteSpace(user.LastName))
                return user.FirstName + " " + user.LastName;
            else
                return user.UserName;
        }

    }
}
