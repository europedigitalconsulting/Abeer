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
using Abeer.Client;

namespace Abeer.Server.Controllers
{
    [Authorize]
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

        public ProfileController(UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService,
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
        }

        [HttpGet]
        public async Task<ActionResult<ApplicationUser>> GetUserProfile()
        {
            var user = await _userManager
                .FindByIdAsync(User.NameIdentifier());

            return Ok(new ViewApplicationUser
            {
                City = user.City,
                Email = user.Email,
                PhotoUrl = string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl,
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
                Title = user.Title
            });
        }

        [HttpPut]
        public async Task<ActionResult<ApplicationUser>> UpdateUser(ApplicationUser applicationUser)
        {
            var user = await _userManager.FindByIdAsync(applicationUser.Id);
            
            user.FirstName = applicationUser.FirstName;
            user.LastName = applicationUser.LastName;
            user.DisplayName = applicationUser.DisplayName;
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

            return BadRequest();
        }

        [HttpGet("PinCode/{id}")]
        public async Task <ActionResult<ApplicationUser>> GeneratePinCode(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            user.PinCode = KeyGenerator.GeneratePinCode(8).ToString();
            user.PinDigit = KeyGenerator.GeneratePinCode(6);

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
    }
}
