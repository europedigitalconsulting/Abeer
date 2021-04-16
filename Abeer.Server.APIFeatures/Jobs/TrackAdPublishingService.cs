using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Abeer.Services.TemplateRenderManager;

namespace Abeer.Server.APIFeatures.Jobs
{
    public class TrackAdPublishingService : IHostedService, IDisposable
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EventTrackingService _eventTrackingService;
        private readonly NotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSenderService _emailSender;
        private readonly ILogger<TrackAdPublishingService> _logger;
        private Timer _timer;

        public TrackAdPublishingService(ILogger<TrackAdPublishingService> logger, IServiceProvider serviceProvider, IWebHostEnvironment env, IConfiguration configuration)
        {
            _logger = logger;

            _scope = serviceProvider.CreateScope();
            _serviceProvider = _scope.ServiceProvider;

            _functionalUnitOfWork = _scope.ServiceProvider.GetRequiredService<FunctionalUnitOfWork>();
            _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _eventTrackingService = _scope.ServiceProvider.GetRequiredService<EventTrackingService>();
            _notificationService = _scope.ServiceProvider.GetRequiredService<NotificationService>();
            _configuration = configuration;
            _env = env;
            _emailSender = _scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Track AdPublishing Service running.");

            _timer = new Timer(async o => await CheckAdPublshing(o), null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        private async Task CheckAdPublshing(object state)
        {
            var today = DateTime.UtcNow.Date;
            var tommorrow = today.AddDays(1);

            var ads = await _functionalUnitOfWork.AdRepository.Where(a => a.StartDisplayTime <= today && a.IsValid);

            foreach (var ad in ads)
            {
                var callbackUrl = $"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ads/details/{ad.Id}";

                var notifications = await _notificationService.GetNotifications(ad.OwnerId, NotificationTypeEnum.AdStartPublished, today, callbackUrl);

                if (!notifications?.Any() == true)
                    await NotifyAd(ad, "startAdPublishing", EmailTemplateEnum.AdStartPublished, NotificationTypeEnum.AdStartPublished, callbackUrl);
            }

            var ads2 = await _functionalUnitOfWork.AdRepository.Where(a => a.EndDisplayTime <= tommorrow && a.IsValid);

            foreach (var ad in ads2)
            {
                var callbackUrl = $"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ads/details/{ad.Id}";

                var notifications = await _notificationService.GetNotifications(ad.OwnerId, NotificationTypeEnum.AdEndPublished, today, callbackUrl);

                if (!notifications?.Any() == true)
                    await NotifyAd(ad, "endAdPublishing", EmailTemplateEnum.AdEndPublished, NotificationTypeEnum.AdEndPublished, callbackUrl);
            }
        }

        private async Task NotifyAd(Shared.Functional.AdModel ad, string eventType, EmailTemplateEnum emailTemplate, NotificationTypeEnum notificationType, string callbackUrl)
        {
            _logger.LogInformation($"notify user {ad.OwnerId} for ad {ad.Id}");

            await _eventTrackingService.Create(ad.OwnerId, "Ad", eventType);

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                CssClass = "advalidate",
                ImageUrl = "alert-advalidate",
                MessageTitle = eventType,
                NotificationIcon = "alert-advalidate",
                NotificationType = notificationType.GetName(),
                NotificationUrl = callbackUrl,
                UserId = ad.OwnerId,
                DisplayMax = 1,
                IsDisplayOnlyOnce = true
            };

            await _notificationService.Create(notification);
            await SendEmailTemplate(ad, emailTemplate, callbackUrl);
        }

        private async Task SendEmailTemplate(Shared.Functional.AdModel ad, EmailTemplateEnum emailType, string callbackUrl)
        {
            _logger.LogInformation($"send email {emailType} to user {ad.OwnerId}");

            var frontWebSite = _configuration["Service:FrontOffice:Url"];
            var logoUrl = $"{frontWebSite.TrimEnd('/')}/assets/img/logo_full.png";
            var unSubscribeUrl = $"{frontWebSite.TrimEnd('/')}/Account/UnSubscribe";

            var parameters = new Dictionary<string, string>()
                        {
                            {"title", ad.Title },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"unSubscribeUrl", unSubscribeUrl },
                            {"callbackUrl", callbackUrl }
                        };

            if(emailType == EmailTemplateEnum.AdEndPublished)
            {
                parameters.Add("EndPublishingDate", ad.EndDisplayTime.Value.ToLongDateString());
                parameters.Add("ViewCount", ad.ViewCount.ToString());
            }

            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, emailType, parameters);
            var user = await _userManager.FindByIdAsync(ad.OwnerId);

            await _emailSender.SendEmailAsync(user.Email, ad.Title, message);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Track Ad publishing service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
