using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abeer.Server.APIFeatures.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileStatisticsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHostEnvironment _env;
        private readonly UrlShortner _urlShortner;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IServiceProvider _serviceProvider;

        public ProfileStatisticsController(UserManager<ApplicationUser> userManager,
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
    }
}
