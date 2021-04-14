using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IServiceProvider _serviceProvider;
        private readonly EventTrackingService _eventTrackingService;

        public ProfileStatisticsController(UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService,
            IServiceProvider serviceProvider,
            IWebHostEnvironment env, IEmailSender emailSender,
            IConfiguration configuration, FunctionalUnitOfWork functionalUnitOfWork, EventTrackingService eventTrackingService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _env = env;
            _emailSender = emailSender;
            _configuration = configuration;
            _functionalUnitOfWork = functionalUnitOfWork;
            _serviceProvider = serviceProvider;
            _eventTrackingService = eventTrackingService;
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

        [HttpGet("evolution")]
        public async Task<ActionResult<IList<StatisticDatePoint>>> GetEvolution()
        {
            if (User.Identity.IsAuthenticated == false)
                return BadRequest();

            var userId = User.NameIdentifier();
            var eventTrackings = (await _eventTrackingService.GetEventTrackingItemsByKey($"ViewProfile#{userId}")).ToList();

            var user = await _userManager.FindByIdAsync(userId);

            if(!string.IsNullOrEmpty(user.PinDigit))
            {
                eventTrackings.AddRange((await _eventTrackingService.GetEventTrackingItemsByKey($"ViewProfile#{user.PinDigit}")));
            }

            var groups = eventTrackings.GroupBy(info => info.CreatedDate.Date)
                    .Select(group => new
                    {
                        Date = group.Key,
                        Count = group.Count()
                    }).OrderBy(x => x.Date);

            var result = groups.Select(g => new StatisticDatePoint { Date = g.Date, Value = g.Count }).ToArray();

            return Ok(result);
        }

        [HttpGet("repartition")]
        public async Task<ActionResult<IList<StatisticDatePoint>>> GetRepartition()
        {
            if (User.Identity.IsAuthenticated == false)
                return BadRequest();

            var userId = User.NameIdentifier();
            var socialNetworks = await _functionalUnitOfWork.SocialNetworkRepository.GetSocialNetworkLinks(userId);

            var eventTrackings = (await _eventTrackingService.GetEventTrackingItemsByKey($"ViewProfile#{userId}")).ToList();

            var user = await _userManager.FindByIdAsync(userId);

            if (!string.IsNullOrEmpty(user.PinDigit))
            {
                eventTrackings.AddRange((await _eventTrackingService.GetEventTrackingItemsByKey($"ViewProfile#{user.PinDigit}")));
            }

            var groups = eventTrackings.GroupBy(info => info.CreatedDate.Date)
                    .Select(group => new
                    {
                        Date = group.Key,
                        Count = group.Count()
                    }).OrderBy(x => x.Date);

            var result = groups.Select(g => new StatisticDatePoint { Date = g.Date, Value = g.Count }).ToArray();

            List<StatisticKeyPoint> repartitions = new List<StatisticKeyPoint>();
            repartitions.AddRange(result.Select(r => new StatisticKeyPoint { Date = r.Date, Key = "Total", Value = r.Value }));

            var dates = repartitions.Where(r => r.Key == "Total").Select(r => r.Date).ToArray();

            foreach (var socialNetwork in socialNetworks)
            {
                var category = $"ViewProfileFromSocial#{socialNetwork.Name.ToLower()}";
                
                var eventTrackingsSocialNetwork = await _eventTrackingService.Where(c=>c.Category == category && c.Key == userId);

                if (eventTrackingsSocialNetwork.Any() == false)
                    continue;

                var geventTrackingSocialNetwork = eventTrackingsSocialNetwork.GroupBy(info => info.CreatedDate.Date)
                        .Select(group => new
                        {
                            Date = group.Key,
                            Count = group.Count()
                        }).OrderBy(x => x.Date);

                var resultSocialNetwork = groups.Select(g => new StatisticDatePoint { Date = g.Date, Value = g.Count }).ToArray();
                                
                foreach(var date in dates)
                {
                    repartitions.Add(new StatisticKeyPoint
                    {
                        Key = socialNetwork.Name,
                        Date = date,
                        Value = resultSocialNetwork.Where(r => r.Date == date).Sum(r => r.Value)
                    });
                }
            }

            foreach(var date in dates)
            {
                var total = repartitions.First(d => d.Date == date).Value;
                var other = repartitions.Where(d => d.Date == date && d.Key != "Total").Sum(d=>d.Value);
                var direct = total - other;
                repartitions.Add(new StatisticKeyPoint { Date = date, Key = "Direct", Value = direct });
            }

            return Ok(repartitions);
        }


        [HttpGet("top10Ads")]
        public async Task<ActionResult<List<AdModel>>> GetTop10Ads()
        {
            if (User.Identity.IsAuthenticated == false)
                return BadRequest();

            var ads = (await _functionalUnitOfWork.AdRepository.Where(a=>a.OwnerId == User.NameIdentifier() && a.IsValid && a.ViewCount > 0))
                .OrderByDescending(a => a.ViewCount);

            return Ok(ads);
        }
    }
}
