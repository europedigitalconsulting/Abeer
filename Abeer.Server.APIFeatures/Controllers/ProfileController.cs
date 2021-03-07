using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Abeer.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Abeer.Shared.ViewModels;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using static Abeer.Services.TemplateRenderManager;
using System;

namespace Abeer.Server.Controllers
{
    [Authorize(Policy = "OnlySubscribers")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _env;
        private readonly UrlShortner _urlShortner;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IServiceProvider _serviceProvider;

        public ProfileController(UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService,
            IServiceProvider serviceProvider,
            IWebHostEnvironment env, UrlShortner urlShortner, IEmailSender emailSender,
            IConfiguration configuration, FunctionalUnitOfWork functionalUnitOfWork)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _env = env;
            _urlShortner = urlShortner;
            _emailSender = emailSender;
            _configuration = configuration;
            _functionalUnitOfWork = functionalUnitOfWork;
            _serviceProvider = serviceProvider;
        }

        [AllowAnonymous]
        [HttpGet("GetUserProfileNoDetail")]
        public async Task<ActionResult<ApplicationUser>> GetUserProfileNoDetail([FromQuery] string userId)
        { 
            if (string.IsNullOrEmpty(userId))
                userId = User.NameIdentifier();

            var user = await _userManager
                .FindByIdAsync(userId);

            var value = new ViewApplicationUser
            {
                City = user.City,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Id = user.Id,
                SocialNetworkConnected = await _functionalUnitOfWork
                    .SocialNetworkRepository
                    .GetSocialNetworkLinks(user.Id) ?? new List<SocialNetwork>(),
                CustomLinks = await _functionalUnitOfWork
                    .CustomLinkRepository
                    .GetCustomLinkLinks(user.Id) ?? new List<CustomLink>(), 
                Country = user.Country,
                Description = user.Description,
                DisplayName = user.DisplayName, 
                LastLogin = user.LastLogin,
                LastName = user.LastName,
                Title = user.Title,
                PhotoUrl = string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl
            };
            return Ok(value);
        }
        [HttpGet]
        public async Task<ActionResult<ApplicationUser>> GetUserProfile([FromQuery] string userId)
        { 
            if (string.IsNullOrEmpty(userId))
                userId = User.NameIdentifier();

            var user = await _userManager
                .FindByIdAsync(userId);

            var value = new ViewApplicationUser
            {
                City = user.City,
                Email = user.Email,
                PinCode = user.PinCode,
                DigitCode = user.PinDigit?.ToString(),
                PhoneNumber = user.PhoneNumber,
                Id = user.Id,
                SocialNetworkConnected = await _functionalUnitOfWork
                    .SocialNetworkRepository
                    .GetSocialNetworkLinks(user.Id) ?? new List<SocialNetwork>(),
                CustomLinks = await _functionalUnitOfWork
                    .CustomLinkRepository
                    .GetCustomLinkLinks(user.Id) ?? new List<CustomLink>(),
                Address = user.Address,
                Country = user.Country,
                Description = user.Description,
                DisplayName = user.DisplayName,
                FirstName = user.FirstName,
                IsAdmin = user.IsAdmin,
                IsManager = user.IsManager,
                IsOnline = user.IsOnline,
                IsOperator = user.IsOperator,
                LastLogin = user.LastLogin,
                LastName = user.LastName,
                Title = user.Title,
                PhotoUrl = string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl
            };
            return Ok(value);
        }

        [HttpGet("daily")]
        public async Task<ActionResult<DailyStatisticsViewModel>> GetDailyStatistcs()
        {
            if (User.Identity.IsAuthenticated == false)
                return BadRequest();

            var ads = await _functionalUnitOfWork.AdRepository.All();
            var userId = User.NameIdentifier();

            var contacts = await _functionalUnitOfWork.ContactRepository.GetContacts(userId);
            var sents = await _functionalUnitOfWork.InvitationRepository.GetInvitationsBy(userId);
            var received = await _functionalUnitOfWork.InvitationRepository.GetInvitationsFor(userId);

            var daily = new DailyStatisticsViewModel
            {
                UserStatistics = new UserStatisticsViewModel
                {
                    NbOfUsers = _userManager.Users.Count(),
                    NbOfUsersOnline = _userManager.Users.Where(u => u.IsOnline).Count()
                },

                AdsStatistics = new AdsStatisticsViewModel
                {
                    AdsCount = ads.Count,
                    AdsOnline = ads.Count(a => a.StartDisplayTime <= DateTime.UtcNow && a.EndDisplayTime >= DateTime.UtcNow && a.IsValid)
                },

                OwnerStatistics = new OwnerStatisticsViewModel
                {
                    ContactsCount = contacts.Count,
                    AdsCount = ads.Count(a => a.OwnerId == userId),
                    InvitationSentCount = sents.Count,
                    InvitationReceivedCount = received.Count
                }
            };

            return Ok(daily);
        }
        
        [HttpPost("SaveNewCard")]
        public async Task<ActionResult<ApplicationUser>> SaveNewCard(ApplicationUser userForm)
        {
            var user = await _userManager.FindByIdAsync(User.NameIdentifier());

            var card = await _functionalUnitOfWork.CardRepository.FirstOrDefault(c => c.CardNumber == userForm.PinDigit);
            if (card == null)
            {
                return NotFound("card not existed");
            }

            else if (card.IsUsed)
            {
                return NotFound("Current is used");
            }

            else if (card.PinCode != userForm.PinCode.ToString())
            {
                return NotFound("Pincode is not valid");
            }
            user.PinCode = userForm.PinCode;
            user.PinDigit = userForm.PinDigit;
            await _userManager.UpdateAsync(user);

            card.IsUsed = true;
            await _functionalUnitOfWork.CardRepository.Update(card);
            return Ok(userForm);

        }
        [HttpPut]
        public async Task<ActionResult<ApplicationUser>> UpdateUser(ApplicationUser applicationUser)
        {
            var user = await _userManager.FindByIdAsync(applicationUser.Id);

            user.FirstName = applicationUser.FirstName;
            user.LastName = applicationUser.LastName;
            user.DisplayName = applicationUser.DisplayName;
            user.Description = applicationUser.Description;
            user.City = applicationUser.City;
            user.Country = applicationUser.Country;
            user.PhotoUrl = applicationUser.PhotoUrl;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(applicationUser);

            return BadRequest();
        }

        [HttpPut("ChangePassword")]
        public async Task<ActionResult<ApplicationUser>> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (changePasswordViewModel.UserId != User.NameIdentifier())
                return BadRequest();

            var user = await _userManager.FindByIdAsync(changePasswordViewModel.UserId);

            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);

            if (result.Succeeded)
                return Ok(user);

            return BadRequest(result.Errors?.FirstOrDefault()?.Code);
        }
        [HttpPut("ChangeEmail")]
        public async Task<ActionResult> ChangeEmail(ChangeMailViewModel changeMailViewModel)
        {
            if (changeMailViewModel.UserId != User.NameIdentifier())
                return BadRequest();

            var mailExist = await _userManager.FindByEmailAsync(changeMailViewModel.NewMail);

            if (mailExist != null)
                return Conflict();

            var user = await _userManager.FindByIdAsync(changeMailViewModel.UserId);
            if (mailExist != null ||user == null || changeMailViewModel.NewMail == user.Email || user.EmailConfirmed == false)
                return NotFound();

            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = Url.Content("~/") },
                    protocol: Request.Scheme);

                var frontWebSite = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
                var logoUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/assets/img/logo_full.png");
                var unSubscribeUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, UriHelper.BuildAbsolute(Request.Scheme, Request.Host, "/Account/UnSubscribe"));

                callbackUrl = await _urlShortner.CreateUrl(Request.Scheme, Request.Host, callbackUrl);

                await SendEmailTemplate(changeMailViewModel.NewMail, "email-confirmation", new Dictionary<string, string>()
                        {
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl }
                        });

                user.EmailConfirmed = false;
                user.UserName = user.Email = changeMailViewModel.NewMail;
                user.NormalizedUserName = user.NormalizedEmail = changeMailViewModel.NewMail.ToUpper();
                await _userManager.UpdateAsync(user);

                return Ok();
            }
            catch (Exception ex)
            { 
                throw;
            } 
        }

        [HttpGet("PinCode/{id}")]
        public async Task<ActionResult<ApplicationUser>> GeneratePinCode(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            user.PinDigit = KeyGenerator.GeneratePinCode(8).ToString();
            user.PinCode = KeyGenerator.GeneratePinCode(6);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(user);

            return BadRequest();
        }

        [HttpGet("Tokens/{id}")]
        public async Task<ActionResult<ApplicationUser>> GenerateTokens(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            user.EncryptionKey = KeyGenerator.GetRandomData(256);
            user.EncryptionIv = KeyGenerator.GetRandomData(128);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(user);

            return BadRequest();
        }

        private async Task SendEmailTemplate(string newMail, string templatePattern, Dictionary<string, string> parameters)
        {
            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, templatePattern, parameters);
            await _emailSender.SendEmailAsync(newMail, "Confirm your email", message);
        }
    }
}
