using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Abeer.Shared.Functional;
using System;
using System.Collections.Concurrent;
using Abeer.Data.UnitOfworks;
using System.Collections.Generic;
using System.Linq;
using Abeer.Services;
using Abeer.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Abeer.Ads.Shared;
using Abeer.Ads.Data;
using Abeer.Shared.Technical;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using static Abeer.Services.TemplateRenderManager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Abeer.Server.APIFeatures.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminDashboardController : ControllerBase
    {
        private readonly FunctionalUnitOfWork functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EventTrackingService _eventTrackingService;
        private readonly NotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly UrlShortner _urlShortner;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSenderService _emailSender;

        private readonly Random rdm = new Random();

        public AdminDashboardController(FunctionalUnitOfWork functionalUnitOfWork, UserManager<ApplicationUser> userManager,
            EventTrackingService eventTrackingService, NotificationService notificationService,
            IConfiguration configuration, UrlShortner urlShortner, IServiceProvider serviceProvider, IWebHostEnvironment env, IEmailSenderService emailSender)
        {
            this.functionalUnitOfWork = functionalUnitOfWork;
            _userManager = userManager;
            _eventTrackingService = eventTrackingService;
            _notificationService = notificationService;
            _configuration = configuration;
            _urlShortner = urlShortner;
            _serviceProvider = serviceProvider;
            _env = env;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardInformation>> GetDashboard()
        {
            var profile = (ViewApplicationUser)User;

            if (!profile.IsAdmin)
                return BadRequest();

            var dashboardInfo = new DashboardInformation
            {
                NbVisits = await _eventTrackingService.Count(e => e.Category == "Navigation" && e.Key == "HomePage"),
                NbRegistar = await _eventTrackingService.Count(e => e.Category == "Register" && e.Key == "Finished"),
                NbLogin = await _eventTrackingService.Count(e => e.Category == "Login"),
                NbRequestSubscription = await _eventTrackingService.Count(e => e.Category == "Subscription" && e.Key.StartsWith("started")),
                NbFinishSubscription = await _eventTrackingService.Count(e => e.Category == "Subscription" && e.Key == "Success"),
                NbAds = await functionalUnitOfWork.AdRepository.CountVisibled()
            };

            var viewFromSocialNetworks = (await _eventTrackingService.Where(c => EF.Functions.Like(c.Category, "%FromSocial%"))).ToList();

            var socialNetworks = functionalUnitOfWork.SocialNetworkRepository.GetNetworks();

            foreach(var socialNetwork in socialNetworks)
            {
                var name = socialNetwork.Name.ToLower();
                
                if (viewFromSocialNetworks.Any(e => e.Key.EndsWith(name)))
                {
                    CommingFromSocialNetwork commingFromSocialNetwork = new CommingFromSocialNetwork
                    {
                        Name = name,
                        Value = viewFromSocialNetworks.Count(e => e.Key.EndsWith(name))
                    };

                    dashboardInfo.CommingFromSocialNetworks.Add(commingFromSocialNetwork);
                }
            }

            List<StatisticsDay> StatisticsDays = new List<StatisticsDay>();

            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var date = startDate;

            while(date < endDate)
            {
                var statisticDay = await ComputeDay(date);
                dashboardInfo.StatisticsDays.Add(statisticDay);
                date = date.AddDays(1);
            }

            return Ok(dashboardInfo);
        }

        private async Task<StatisticsDay> ComputeDay(DateTime date)
        {
            var statisticDay = new StatisticsDay();
            var tommorrow = date.AddDays(1);

            statisticDay.Date = date;
            statisticDay.NbVisits = await _eventTrackingService.Count(e => e.Category == "Navigation" && e.Key == "HomePage" && e.CreatedDate >= date && e.CreatedDate < tommorrow);
            statisticDay.NbRegistar = await _eventTrackingService.Count(e => e.Category == "Register" && e.Key == "Finished" && e.CreatedDate >= date && e.CreatedDate < tommorrow);
            statisticDay.NbLogin = await _eventTrackingService.Count(e => e.Category == "Login" && e.CreatedDate >= date && e.CreatedDate < tommorrow);
            statisticDay.NbRequestSubscription = await _eventTrackingService.Count(e => e.Category == "Subscription" && EF.Functions.Like(e.Key, "started%") && e.CreatedDate >= date && e.CreatedDate < tommorrow);
            statisticDay.NbFinishSubscription = await _eventTrackingService.Count(e => e.Category == "Subscription" && e.Key == "Success" && e.CreatedDate >= date && e.CreatedDate < tommorrow);
            statisticDay.NbAds = await functionalUnitOfWork.AdRepository.CountVisibled(date);

            return statisticDay;
        }
    }
}
