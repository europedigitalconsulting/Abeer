using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.Technical;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SQLitePCL;
using Abeer.Services;
using static Abeer.Services.TemplateRenderManager;
using Abeer.Shared.Security;
using System.Security.Claims;
using Abeer.Data.UnitOfworks;

namespace Abeer.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly UrlShortner _urlShortner;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IWebHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider;
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;

        public ProfileService(UserManager<ApplicationUser> userManager,
            NotificationService notificationService, IConfiguration configuration, UrlShortner urlShortner,
            IEmailSenderService emailSenderService, IWebHostEnvironment env, IServiceProvider serviceProvider, FunctionalUnitOfWork functionalUnitOfWork)
        {
            _userManager = userManager;
            _notificationService = notificationService;
            _configuration = configuration;
            _urlShortner = urlShortner;
            _emailSenderService = emailSenderService;
            _env = env;
            _serviceProvider = serviceProvider;
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var nameClaim = context.Subject.FindAll(JwtClaimTypes.Name);
            context.IssuedClaims.AddRange(nameClaim);

            var roleClaims = context.Subject.FindAll(JwtClaimTypes.Role);
            context.IssuedClaims.AddRange(roleClaims);

            bool isReadonly = false;

            var user = await _userManager.FindByNameAsync(context.Subject.Identity.Name);

            if (user != null)
            {

                if (user.IsUnlimited == false)
                {
                    var notifications = await _notificationService.GetNotifications(user.Id, "daily-reminder");

                    if (notifications.Any(n => n.CreatedDate.Date.Equals(DateTime.UtcNow.Date)) == false)
                    {
                        await _notificationService.Create(user.Id, "daily-reminder", "subscription-pack", "reminder", "reminder", "reminder", "daily-reminder");
                    }

                    if (user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate < DateTime.UtcNow)
                    {
                        notifications = await _notificationService.GetNotifications(user.Id, "expiredprofile");

                        if (notifications.Any(n => n.CreatedDate.Date.Equals(DateTime.UtcNow.Date)) == false)
                        {
                            await _notificationService.Create(user.Id, "expiredprofile", "subscription-pack", "reminder", "reminder", "reminder", "expiredprofile");
                            isReadonly = true;
                            await SendEmailTemplate(user, "expiredprofile", "expiredprofile");
                        }
                    }
                    else if (user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value.Subtract(DateTime.UtcNow).Days <= 5)
                    {
                        notifications = await _notificationService.GetNotifications(user.Id, "soonexpireprofile");

                        if (notifications.Any(n => n.CreatedDate.Date.Equals(DateTime.UtcNow.Date)) == false)
                        {
                            await _notificationService.Create(user.Id, "soonexpireprofile", "subscription-pack", "reminder", "reminder", "reminder", "soonexpireprofile");
                            await SendEmailTemplate(user, "soonexpireprofile", "soonexpireprofile");
                        }
                    }
                }

                if (!user.IsOnline)
                {
                    user.IsOnline = true;
                    user.LastLogin = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                AddClaim(context, ClaimNames.Title, user.Title);
                AddClaim(context, ClaimNames.Address, user.Address);
                AddClaim(context, ClaimNames.City, user.City);
                AddClaim(context, ClaimNames.Country, user.Country);
                AddClaim(context, ClaimNames.DisplayDescription, user.DisplayDescription);
                AddClaim(context, ClaimNames.DescriptionVideo, user.DescriptionVideo);
                AddClaim(context, ClaimNames.DescriptionVideoCover, user.DescriptionVideoCover);
                AddClaim(context, ClaimNames.VideoProfileUrl, user.VideoProfileUrl);
                AddClaim(context, ClaimNames.VideProfileCoverUrl, user.VideProfileCoverUrl);
                AddClaim(context, ClaimNames.Description, user.Description);
                AddClaim(context, ClaimNames.DigitCode, user.PinCode);
                AddClaim(context, ClaimNames.DisplayName, GetDisplayName(user));
                AddClaim(context, ClaimNames.Email, user.Email);
                AddClaim(context, ClaimNames.FirstName, user.FirstName);
                AddClaim(context, ClaimNames.Id, user.Id);
                AddClaim(context, ClaimNames.PhoneNumber, user.PhoneNumber);

                AddRole(context, ClaimNames.IsAdmin, user.IsAdmin);
                AddRole(context, ClaimNames.IsManager, user.IsManager);
                AddRole(context, ClaimNames.IsOperator, user.IsOperator);
                AddRole(context, ClaimNames.IsOnline, user.IsOnline);

                AddClaim(context, ClaimNames.LastLogin, user.LastLogin);
                AddClaim(context, ClaimNames.LastName, user.LastName);
                AddClaim(context, ClaimNames.NumberOfView, user.NubmerOfView);
                AddClaim(context, ClaimNames.PhotoUrl, user.PhotoUrl);
                AddClaim(context, ClaimNames.PinCode, user.PinCode);
                AddClaim(context, ClaimNames.SubscriptionStart, user.SubscriptionStartDate);
                AddClaim(context, ClaimNames.SubscriptionEnd, user.SubscriptionEndDate);

                if(!user.IsAdmin && !user.IsUnlimited)
                {
                    var subscription = await _functionalUnitOfWork.SubscriptionRepository.GetLatestSubscriptionForUser(user.Id);
                    var pack = await _functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(s => s.Id == subscription.SubscriptionPackId);

                    if (subscription != null)
                        AddClaim(context, ClaimNames.Subscription, pack.Label.ToLower());
                }

                AddClaim(context, ClaimNames.IsReadOnly, user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value <= DateTime.UtcNow);
            }
        }

        private void AddClaim(ProfileDataRequestContext context, string name, object value)
        {
            if (value == null)
                return;

            if (context.IssuedClaims.Any(c => c.Type == name))
                return;

            context.IssuedClaims.Add(new System.Security.Claims.Claim(name, value.ToString()));
        }

        private static void AddRole(ProfileDataRequestContext context, string name, bool isAllowed)
        {
            if (!isAllowed)
                return;

            if (context.IssuedClaims.Any(c=>c.Type == ClaimTypes.Role && c.Value == name) == false)
                context.IssuedClaims.Add(new Claim(ClaimTypes.Role, name));
        }


        private static string GetDisplayName(ApplicationUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.DisplayName))
                return user.DisplayName;
            else if (!string.IsNullOrWhiteSpace(user.LastName))
                return user.FirstName + " " + user.LastName;
            else
                return user.UserName;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }

        private async Task SendEmailTemplate(ApplicationUser user, string emailTemplate, string emailSubject)
        {
            var longUrl = $"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/subscription-pack";

            var frontWebSite = _configuration["Service:FrontOffice:Url"];
            var logoUrl = $"{_configuration["Service:FrontOffice:Url"]}/assets/img/logo_full.png";
            var login = $"{user.DisplayName}";

            var code = GenerateCode();
            var shortedUrl = $"{_configuration["Service:FrontOffice:Url"].TrimEnd('/')}/shortned/{code}";

            var callbackUrl = await _urlShortner.CreateUrl(false, false, shortedUrl, longUrl, null, code);

            var parameters = new Dictionary<string, string>()
                        {
                            {"login", login },
                            {"frontWebSite", frontWebSite },
                            {"logoUrl", logoUrl },
                            {"callbackUrl", callbackUrl }
                        };

            var message = GenerateHtmlTemplate(_serviceProvider, _env.WebRootPath, emailTemplate, parameters);
            await _emailSenderService.SendEmailAsync(user.Email, emailSubject, message);
        }

        private static string GenerateCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];

            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}
