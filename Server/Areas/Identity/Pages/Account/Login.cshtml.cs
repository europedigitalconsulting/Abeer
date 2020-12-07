using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared;

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
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UrlShortner _urlShortner;
        private readonly IWebHostEnvironment _env;
        private readonly string _webRoot;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger, FunctionalUnitOfWork functionalUnitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public IConfiguration Configuration { get; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
        public class InputExternalModel
        {
            public int PinCode { get; set; }

            public string PinDigit { get; set; }
        }
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            InputExternalModel Input = new InputExternalModel();

            if (!string.IsNullOrEmpty(Request.Query?["PinDigit"]))
            {
                Input.PinCode = int.Parse(Request.Query?["PinDigit"]);
            }

            if (Input.PinCode > 0)
            {
                var user = await _userManager.Users.Where(u => u.PinDigit == Input.PinDigit).ToListAsync();

                if (user == null || user.Count == 0)
                    return Redirect($"./Register?PinDigit={Request.Query?["PinDigit"]}");
            }

            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    if (user != null)
                    {
                        user.IsOnline = true;
                        user.LastLogin = DateTime.Now;
                        await _userManager.UpdateAsync(user);
                    }
                    _logger.LogInformation("User logged in.");

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

                    if (!string.IsNullOrWhiteSpace(user.Title))
                        identity.AddClaim(new Claim("title", user.Title));

                    if (!string.IsNullOrWhiteSpace(user.Description))
                        identity.AddClaim(new Claim("description", user.Description));

                    if (!string.IsNullOrWhiteSpace(user.Country))
                        identity.AddClaim(new Claim("country", user.Country));

                    if (!string.IsNullOrWhiteSpace(user.City))
                        identity.AddClaim(new Claim("city", user.City));

                    if (!string.IsNullOrWhiteSpace(user.PhotoUrl))
                        identity.AddClaim(new Claim("photourl", user.PhotoUrl));

                    identity.AddClaim(new Claim("numberofview", user.NubmerOfView.ToString()));

                    if (!string.IsNullOrWhiteSpace(user.Address))
                        identity.AddClaim(new Claim("address", user.Address));

                    identity.AddClaim(new Claim("IsUnlimited", user.IsUnlimited.ToString()));

                    if (user.IsAdmin)
                        identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));

                    if (user.IsManager)
                        identity.AddClaim(new Claim(ClaimTypes.Role, "manager"));


                    identity.AddClaim(new Claim("subscribeStart", (user?.SubscriptionStartDate != null) ? user.SubscriptionStartDate.Value.ToString() : ""));
                    if (user.SubscriptionEndDate != null)
                        identity.AddClaim(new Claim("subscribeEnd", (user?.SubscriptionEndDate != null) ? user.SubscriptionEndDate.Value.ToString() : ""));

                    if (user.IsOperator)
                        identity.AddClaim(new Claim(ClaimTypes.Role, "operator"));

                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("Identity.Application", principal);

                    return LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
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
