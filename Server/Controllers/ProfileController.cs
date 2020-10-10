using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Abeer.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

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

        public ProfileController(UserManager<ApplicationUser> userManager,
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

        [HttpGet]
        public async Task<ActionResult<ApplicationUser>> GetUserProfile()
        {
            var user = await _userManager.FindByIdAsync(User.NameIdentifier());
            return Ok(user);
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


            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
                return Ok(applicationUser);

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
