using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Abeer.Shared;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Hosting;
using static Abeer.Services.TemplateRenderManager;
using Abeer.Services;

namespace Abeer.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSenderService _emailSender;
        private readonly UrlShortner _urlShortner;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env, IEmailSenderService emailSender, UrlShortner urlShortner, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _urlShortner = urlShortner;
            _serviceProvider = serviceProvider;
            _env = env;
        }

        [BindProperty(SupportsGet =true)]
        public string ReturnUrl { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool ConfirmSentEmail { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, ReturnUrl },
                    protocol: Request.Scheme);

                var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
                var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
                var unSubscribeUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe"));

                await SendEmailTemplate("Password-reset", new Dictionary<string, string>()
                    {
                        {"frontWebSite", frontWebSite },
                        {"logoUrl", logoUrl },
                        {"unSubscribeUrl", unSubscribeUrl },
                        {"callbackUrl", callbackUrl }
                    });

                ConfirmSentEmail = true;
            }

            return Page();
        }

        private async Task SendEmailTemplate(string templatePattern, Dictionary<string, string> parameters)
        {
            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, templatePattern, parameters);
            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", message);
        }

    }
}
