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
using Abeer.Shared.Security;

namespace Abeer.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSenderService _emailSender;
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
            IEmailSenderService emailSender,
            IConfiguration configuration,
            UrlShortner urlShortner, CountriesService countriesService, FunctionalUnitOfWork functionalUnitOfWork, EventTrackingService eventTrackingService)
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
            _eventTrackingService = eventTrackingService;
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

            public bool HasCard { get; set; }
            public int PinCode { get; set; }
            public string DigitCode { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
        }

        private static Random rdm = new Random();
        private readonly EventTrackingService _eventTrackingService;

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (Input == null)
                Input = new InputModel();

            if (Request?.Query?.ContainsKey("PinCode") == true)
                Input.PinCode = int.Parse(Request.Query["PinCode"]);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Category = "Register",
                Key = "Start"
            });

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (Input.HasCard && (Input?.PinCode.ToString().Length != 5 || string.IsNullOrEmpty(Input.DigitCode)))
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
                PinDigit = Input.HasCard ? Input.DigitCode : string.Empty,
                PinCode = Input.HasCard ? Input.PinCode : -1,
                City = Input.City,
                Country = Input.Country,
                SubscriptionStartDate = DateTime.Now,
                SubscriptionEndDate = DateTime.Now.AddDays(5)
            };

            _logger.LogInformation($"start get card {Input.PinCode}");

            var card = await _functionalUnitOfWork.CardRepository.FirstOrDefault(c => c.CardNumber == Input.DigitCode);

            if (Input.HasCard)
            {
                if (card == null)
                {
                    ModelState.AddModelError("", "card not existed");
                    return Page();
                }

                else if (card.IsUsed)
                {
                    ModelState.AddModelError("", "Current is used");
                    return Page();
                }

                else if (card.PinCode != Input.PinCode.ToString())
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
                var login = $"{Input.Email}";

                callbackUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, callbackUrl);

                _logger.LogInformation($"Send Email confirmation to {Input.Email}.");

                await SendEmailTemplate("email-confirmation", new Dictionary<string, string>()
                        {
                            {"login", login },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl }
                        });

                await _eventTrackingService.Create(new Shared.Functional.EventTrackingItem
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow,
                    Category = "Register",
                    Key = "Created",
                    UserId = user.Id
                });


                _logger.LogInformation("set card is used");

                if (Input.HasCard)
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
