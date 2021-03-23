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
using Microsoft.CodeAnalysis.CSharp;

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
        private readonly EventTrackingService _eventTrackingService;

        public ProfileController(UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService,
            IServiceProvider serviceProvider,
            IWebHostEnvironment env, UrlShortner urlShortner, IEmailSender emailSender,
            IConfiguration configuration, FunctionalUnitOfWork functionalUnitOfWork, EventTrackingService eventTrackingService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _env = env;
            _urlShortner = urlShortner;
            _emailSender = emailSender;
            _configuration = configuration;
            _functionalUnitOfWork = functionalUnitOfWork;
            _serviceProvider = serviceProvider;
            _eventTrackingService = eventTrackingService;
        }

        [AllowAnonymous]
        [HttpGet("GetUserProfileNoDetail")]
        public async Task<ActionResult<ApplicationUser>> GetUserProfileNoDetail([FromQuery]string userId)
        {
            if (string.IsNullOrEmpty(userId))
                userId = User.NameIdentifier();

            var user = await _userManager
                .FindByIdAsync(userId);


            user.NubmerOfView += 1;
            await _userManager.UpdateAsync(user);

            if (!User.Identity.IsAuthenticated)
                await _eventTrackingService.Create(User.NameIdentifier(), "ViewProfile", userId);
            else
                await _eventTrackingService.Create(null, "ViewProfile", userId);

            var view = (ViewApplicationUser)user;

            view.NumberOfContacts = (await _functionalUnitOfWork.ContactRepository.GetContacts(userId)).Count;
            view.NumberOfAds = (await _functionalUnitOfWork.AdRepository.GetVisibledUser(userId)).Count;

            view.SocialNetworkConnected = await _functionalUnitOfWork
                .SocialNetworkRepository
                .GetSocialNetworkLinks(user.Id) ?? new List<SocialNetwork>();

            view.CustomLinks = await _functionalUnitOfWork
                    .CustomLinkRepository
                    .GetCustomLinkLinks(user.Id) ?? new List<CustomLink>();

            view.PhotoUrl = string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl;

            return Ok(view);
        }
        [HttpGet]
        public async Task<ActionResult<ApplicationUser>> GetUserProfile([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                userId = User.NameIdentifier();

            var user = await _userManager
                .FindByIdAsync(userId);

            user.NubmerOfView += 1;
            await _userManager.UpdateAsync(user);

            if (!User.Identity.IsAuthenticated)
                await _eventTrackingService.Create(User.NameIdentifier(), "ViewProfile", userId);
            else
                await _eventTrackingService.Create(null, "ViewProfile", userId);

            var view = (ViewApplicationUser)user;

            view.NumberOfContacts = (await _functionalUnitOfWork.ContactRepository.GetContacts(userId)).Count;
            view.NumberOfAds = (await _functionalUnitOfWork.AdRepository.GetVisibledUser(userId)).Count;

            view.SocialNetworkConnected = await _functionalUnitOfWork
                .SocialNetworkRepository
                .GetSocialNetworkLinks(user.Id) ?? new List<SocialNetwork>();

            view.CustomLinks = await _functionalUnitOfWork
                    .CustomLinkRepository
                    .GetCustomLinkLinks(user.Id) ?? new List<CustomLink>();

            view.PhotoUrl = string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl;

            view.IsReadOnly = (user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate < DateTime.UtcNow);

            return Ok(view);
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
        public async Task<ActionResult<ApplicationUser>> UpdateUser(ViewApplicationUser applicationUser)
        {
            var user = await _userManager.FindByIdAsync(applicationUser.Id);

            user.Title = applicationUser.Title;
            user.Address = applicationUser.Address;
            user.City = applicationUser.City;
            user.Country = applicationUser.Country;
            user.DisplayDescription = applicationUser.DisplayDescription;
            user.DescriptionVideo = applicationUser.DescriptionVideo;
            user.DescriptionVideoCover = applicationUser.DescriptionVideoCover;
            user.Description = applicationUser.Description;
            user.PinDigit = applicationUser.DigitCode;
            user.PinCode = applicationUser.PinCode;
            user.DisplayName = applicationUser.DisplayName;
            user.FirstName = applicationUser.FirstName;
            user.PhoneNumber = applicationUser.PhoneNumber;
            user.IsAdmin = applicationUser.IsAdmin;
            user.IsManager = applicationUser.IsManager;
            user.IsOnline = applicationUser.IsOnline;
            user.IsOperator = applicationUser.IsOperator;
            user.LastLogin = applicationUser.LastLogin;
            user.LastName = applicationUser.LastName;
            user.NubmerOfView = applicationUser.NumberOfView;
            user.PhotoUrl = applicationUser.PhotoUrl;
            user.VideoProfileUrl = applicationUser.VideoProfileUrl;
            user.VideProfileCoverUrl = applicationUser.VideProfileCoverUrl;
            user.SubscriptionStartDate = user.SubscriptionStartDate;
            user.SubscriptionEndDate = user.SubscriptionEndDate;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(applicationUser);

            return BadRequest();
        }

        [HttpPut("ChangePassword")]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (changePasswordViewModel.UserId != User.NameIdentifier())
                return BadRequest();

            var user = await _userManager.FindByIdAsync(changePasswordViewModel.UserId);

            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);

            if (result.Succeeded)
                return Ok(user);

            return NotFound();
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
            if (mailExist != null || user == null || changeMailViewModel.NewMail == user.Email || user.EmailConfirmed == false)
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
