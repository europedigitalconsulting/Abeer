﻿using System;
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

        public ProfileService(UserManager<ApplicationUser> userManager, 
            NotificationService notificationService, IConfiguration configuration, UrlShortner urlShortner, 
            IEmailSenderService emailSenderService, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _notificationService = notificationService;
            _configuration = configuration;
            _urlShortner = urlShortner;
            _emailSenderService = emailSenderService;
            _env = env;
            _serviceProvider = serviceProvider;
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

                    if(user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate < DateTime.UtcNow)
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
                foreach (var claim in context.Subject.Claims)
                {
                    if (!context.IssuedClaims.Any(c => c.Type == claim.Type))
                        context.IssuedClaims.Add(claim);
                }

                if (!context.IssuedClaims.Any(c => c.Type == "subscribeStart") && user.SubscriptionStartDate.HasValue)
                    context.IssuedClaims.Add(new System.Security.Claims.Claim("subscribeStart", user.SubscriptionStartDate.Value.ToString()));

                if (!context.IssuedClaims.Any(c => c.Type == "subscribeEnd") && user.SubscriptionEndDate.HasValue)
                    context.IssuedClaims.Add(new System.Security.Claims.Claim("subscribeEnd", user.SubscriptionEndDate.Value.ToString()));

                if (!context.IssuedClaims.Any(c => c.Type == "photoUrl"))
                    context.IssuedClaims.Add(new System.Security.Claims.Claim("photoUrl", (string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl)));

                if (!context.IssuedClaims.Any(c => c.Type == "readonly") && isReadonly)
                    context.IssuedClaims.Add(new System.Security.Claims.Claim("readonly", "true"));
            }
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
