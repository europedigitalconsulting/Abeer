using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using Abeer.Services;
using Abeer.Shared;
using static Abeer.Services.TemplateRenderManager;
using Microsoft.AspNetCore.Mvc.Rendering;
using Abeer.Data.UnitOfworks;

namespace Abeer.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UrlShortner _urlShortner;
        private readonly CountriesService countriesService;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly string _webRoot;

        public RegisterModel(
            IServiceProvider serviceProvider,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IWebHostEnvironment env,
            IEmailSender emailSender,
            IConfiguration configuration,
            UrlShortner urlShortner, CountriesService countriesService, FunctionalUnitOfWork functionalUnitOfWork)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            Configuration = configuration;
            _urlShortner = urlShortner;
            this.countriesService = countriesService;
            _functionalUnitOfWork = functionalUnitOfWork;
            _env = env;
            _webRoot = _env.WebRootPath;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<SelectListItem> Countries => GetCountries();

        private List<SelectListItem> GetCountries()
        {
            var countries = countriesService.GetCountries(CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower()).Result
                .Select(c => new SelectListItem(c.Name, c.Estatcode)).ToList();

            return countries;
        }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public IConfiguration Configuration { get; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public bool NoCard { get; set; }
            public int PinCode { get; set; }
            public string DigitCode { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
        }

        static Random rdm = new Random();

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (Input == null)
                Input = new InputModel();

            Input.DigitCode = rdm.Next(10000, 99999).ToString();

            if (Request?.Query?.ContainsKey("PinCode") == true)
                Input.PinCode = int.Parse(Request.Query["PinCode"]);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!Input.NoCard && Input.PinCode > 0 || string.IsNullOrEmpty(Input.DigitCode))
            {
                ModelState.AddModelError("", "DigitCode/PinCode required");
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                DisplayName = GetDisplayName(),
                PinDigit = Input.NoCard ? string.Empty : Input.DigitCode,
                PinCode = Input.NoCard ? -1 : Input.PinCode,
                City = Input.City,
                Country = Input.Country,
                SubscriptionStartDate = DateTime.Now,
                SubscriptionEndDate = DateTime.Now.AddDays(5)
            };

            _logger.LogInformation($"start get card {Input.PinCode}");

            var card = await _functionalUnitOfWork.CardRepository.FirstOrDefault(c => c.CardNumber == Input.DigitCode);

            if (!Input.NoCard)
            {
                if (card == null)
                {
                    ModelState.AddModelError("", "card not existed");
                    return Page();
                }

                else if (card.IsUsed)
                {
                    ModelState.AddModelError("", "Card is used");
                    return Page();
                }

                else if (card.PinCode != Input.DigitCode)
                {
                    ModelState.AddModelError("PinCode", "Pincode is not valid");
                    return Page();
                }
            }

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
                var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
                var unSubscribeUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe"));

                callbackUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, callbackUrl);

                _logger.LogInformation($"Send Email confirmation to {Input.Email}.");

                await SendEmailTemplate("email-confirmation", new Dictionary<string, string>()
                        {
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl }
                        });

                _logger.LogInformation("set card is used");

                if (!Input.NoCard)
                {
                    card.IsUsed = true;
                    await _functionalUnitOfWork.CardRepository.Update(card);
                }

                _logger.LogInformation("Redirect to register confirmation page");

                return RedirectToPage("RegisterConfirmation",
                    new { email = Input.Email, returnUrl = returnUrl });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError(error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task SendEmailTemplate(string templatePattern, Dictionary<string, string> parameters)
        {
            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, templatePattern, parameters);
            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", message);
        }

        private string GetDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(Input.DisplayName))
                return Input.DisplayName;

            if (!string.IsNullOrWhiteSpace(Input.LastName))
            {
                var parts = new List<string>
                {
                    Input.LastName
                };

                if (!string.IsNullOrWhiteSpace(Input.FirstName))
                    parts.Add(Input.FirstName);

                return string.Join(" ", parts);
            }
            else
                return Input.Email;
        }
    }
}
