using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using Microsoft.AspNetCore.Http.Extensions;
using Abeer.Services;
using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Abeer.Server.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _env;
        private readonly UrlShortner _urlShortner;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService,
            IWebHostEnvironment env, UrlShortner urlShortner, IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _env = env;
            _urlShortner = urlShortner;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
        {
            var data = await _userManager.Users.ToListAsync();

            if (data.Any())
            {
                foreach (var user in data)
                {
                    user.IsLocked = await _userManager.IsLockedOutAsync(user);
                }
            }

            return data;
        }

        [HttpGet("suggestions")]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> Suggestions(string term, string roles)
        {
            var users = await _userManager.Users.Where(u => u.DisplayName.Contains(term)
                || u.Email.Contains(term) || u.FirstName.Contains(term) || u.LastName.Contains(term)
                || u.PinCode.Contains(term) || u.UserName.Contains(term)).ToListAsync();

            if (string.IsNullOrWhiteSpace(roles))
                return users;

            ConcurrentBag<ApplicationUser> result = new ConcurrentBag<ApplicationUser>();

            Parallel.ForEach(users, (ApplicationUser user) =>
            {
                if (string.IsNullOrWhiteSpace(roles))
                    result.Add(user);
                else if (roles.Contains("admin", StringComparison.OrdinalIgnoreCase) && user.IsAdmin)
                    result.Add(user);
                else if (roles.Contains("operator", StringComparison.OrdinalIgnoreCase) && user.IsOperator)
                    result.Add(user);
                else if (roles.Contains("manager", StringComparison.OrdinalIgnoreCase) && user.IsManager)
                    result.Add(user);
            });

            return result.ToList();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUser>> GetUser(string id)
        {
            var User = await _userManager.FindByIdAsync(id);

            if (User == null)
            {
                return NotFound();
            }

            return User;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, ApplicationUser User)
        {
            if (id != User.Id)
            {
                return BadRequest();
            }

            var applicationUser = await _userManager.FindByIdAsync(id);

            if (applicationUser == null)
                return NotFound();

            try
            {
                applicationUser.FirstName = User.FirstName;
                applicationUser.LastName = User.LastName;
                applicationUser.DisplayName = User.DisplayName;
                applicationUser.UserName = User.UserName;
                applicationUser.NormalizedUserName = User.UserName.ToUpperInvariant();

                applicationUser.IsAdmin = User.IsAdmin;
                applicationUser.IsManager = User.IsManager;
                applicationUser.IsOperator = User.IsOperator;

                applicationUser.City = User.City;
                applicationUser.Country = User.Country;
                applicationUser.PinCode = User.PinCode;
                applicationUser.PinDigit = User.PinDigit;

                if (applicationUser.EmailConfirmed && !applicationUser.Email.Equals(User.Email))
                {
                    applicationUser.EmailConfirmed = false;
                    await SendChangeEmailConfirm(applicationUser);
                }

                await _userManager.UpdateAsync(applicationUser);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private async Task SendChangeEmailConfirm(ApplicationUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            var templatePath = Path.Combine(_env.WebRootPath,
                "Templates",
                "Email",
                $"email-change-email.{CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower()}.html");

            if (!System.IO.File.Exists(templatePath))
                templatePath = Path.Combine(_env.WebRootPath,
                "Templates",
                "Email",
                $"email-change-email.html");

            string htmlBody = "";

            using (StreamReader SourceReader = System.IO.File.OpenText(templatePath))
            {
                htmlBody = SourceReader.ReadToEnd();
            }

            var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
            var unSubscribeUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe"));
            callbackUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, callbackUrl);

            var message = htmlBody.Replace("[frontWebSite]", frontWebSite, StringComparison.OrdinalIgnoreCase);
            message = message.Replace("[LogoUrl]", logoUrl, StringComparison.OrdinalIgnoreCase);
            message = message.Replace("[CallBackUrl]", callbackUrl, StringComparison.OrdinalIgnoreCase);
            message = message.Replace("[UnSubscribeUrl]", unSubscribeUrl, StringComparison.OrdinalIgnoreCase);
            message = message.Replace("[PostalAddress]", _configuration["Company:PostalAddress"]);

            await _emailSender.SendEmailAsync(user.Email, "email_change-email_subject", message);
        }

        // POST: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        //[Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<ActionResult<ApplicationUser>> PostUser(ApplicationUser user)
        {
            var temporaryPassword = "Xc9wf8or&";// GeneratePassword();
            var result = await _userManager.CreateAsync(user, temporaryPassword);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);

                var templatePath = Path.Combine(_env.WebRootPath,
                    "Templates",
                    "Email",
                    $"email-confirmation.{CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower()}.html");

                if (!System.IO.File.Exists(templatePath))
                    templatePath = Path.Combine(_env.WebRootPath,
                    "Templates",
                    "Email",
                    $"email-confirmation.html");

                string htmlBody = "";

                using (StreamReader SourceReader = System.IO.File.OpenText(templatePath))
                {
                    htmlBody = SourceReader.ReadToEnd();
                }

                var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
                var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
                var unSubscribeUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe"));
                callbackUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, callbackUrl);

                var message = htmlBody.Replace("[frontWebSite]", frontWebSite, StringComparison.OrdinalIgnoreCase);

                message = message.Replace("[LogoUrl]", logoUrl, StringComparison.OrdinalIgnoreCase);
                message = message.Replace("[CallBackUrl]", callbackUrl, StringComparison.OrdinalIgnoreCase);
                message = message.Replace("[UnSubscribeUrl]", unSubscribeUrl, StringComparison.OrdinalIgnoreCase);
                message = message.Replace("[PostalAddress]", _configuration["Company:PostalAddress"]);
                message = message.Replace("[TempPassword]", temporaryPassword);
                message = message.Replace("[Login]", user.UserName);

                await _emailSender.SendEmailAsync(user.Email, "email_created_subject", message);

                return user;
            }

            return BadRequest();
        }

        static readonly Random rnd = new Random();

        private string GeneratePassword()
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            const int length = 8;

            var res = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }

            return res.ToString();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationUser>> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.IsOnline)
                return Forbid();

            await _userManager.DeleteAsync(user);

            return user;
        }

        [HttpPut("validate/{id}")]
        public async Task<ActionResult<ApplicationUser>> ValidateUser(string id, ApplicationUser applicationUser)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.IsOnline)
                return Forbid();

            if (user.Id != applicationUser.Id)
                return BadRequest();

            if (user.Id == User.NameIdentifier())
                return BadRequest();

            if (await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

            return user;
        }

        [HttpPut("Lock/{id}")]
        public async Task<ActionResult<ApplicationUser>> LockUser(string id, ApplicationUser applicationUser)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.IsOnline)
                return Forbid();

            if (user.Id != applicationUser.Id)
                return BadRequest();

            if (user.Id == User.NameIdentifier())
                return BadRequest();

            var result = await _userManager.SetLockoutEnabledAsync(user, true);

            if (result.Succeeded)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            return user;
        }

        [HttpPut("UnLock/{id}")]
        public async Task<ActionResult<ApplicationUser>> UnLockUser(string id, ApplicationUser applicationUser)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.IsOnline)
                return Forbid();

            if (user.Id != applicationUser.Id)
                return BadRequest();

            if (user.Id == User.NameIdentifier())
                return BadRequest();

            var result = await _userManager.SetLockoutEnabledAsync(user, false);

            if (result.Succeeded)
                await _userManager.ResetAccessFailedCountAsync(user);

            return user;
        }

        private async Task<bool> UserExists(string id)
        {
            return (await _userManager.FindByIdAsync(id)) != null;
        }
    }
}
