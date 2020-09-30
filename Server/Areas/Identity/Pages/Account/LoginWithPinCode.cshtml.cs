using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abeer.Services;
using Abeer.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using IdentityModel;
using Abeer.Services;

namespace Abeer.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWithPinCodeModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UrlShortner _urlShortner;
        private readonly IWebHostEnvironment _env;
        private readonly string _webRoot;

        public LoginWithPinCodeModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IWebHostEnvironment env,
            IEmailSender emailSender,
            IConfiguration configuration, UrlShortner urlShortner)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            Configuration = configuration;
            _urlShortner = urlShortner;
            _env = env;
            _webRoot = _env.WebRootPath;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public IConfiguration Configuration { get; }

        public class InputModel
        {
            [DataType(DataType.Password)]
            [Display(Name = "Pin Code")]
            public string PinCode { get; set; }
            
            [DataType(DataType.Password)]
            [Display(Name = "Pin Digit"), MaxLength(6)]
            public string PinDigit { get; set; }
        }

        public Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PinCode == Input.PinCode && u.PinDigit == int.Parse(Input.PinDigit));
                
                if(User == null)
                {
                    ModelState.AddModelError("", "User is not authorized");
                    return Page();
                }

                user.IsOnline = true;
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var identity = new ClaimsIdentity("Identity.Application");


                identity.AddClaim(new Claim(JwtClaimTypes.Subject, user.Id));
                identity.AddClaim(new Claim(JwtClaimTypes.Name, user.UserName));
                
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Email));

                if (!string.IsNullOrWhiteSpace(user.FirstName))
                    identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));

                if (!string.IsNullOrWhiteSpace(user.LastName))
                    identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

                identity.AddClaim(new Claim("displayname", GetDisplayName(user)));

                identity.AddClaim(new Claim("isonline", user.IsOnline.ToString()));
                identity.AddClaim(new Claim("lastlogin", user.LastLogin.ToString("s")));

                if (user.IsAdmin)
                    identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));

                if (user.IsManager)
                    identity.AddClaim(new Claim(ClaimTypes.Role, "manager"));

                if (user.IsOperator)
                    identity.AddClaim(new Claim(ClaimTypes.Role, "operator"));

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
