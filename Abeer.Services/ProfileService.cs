using System;
using System.Linq;
using System.Threading.Tasks;
using Abeer.Shared;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using SQLitePCL;

namespace Abeer.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public ProfileService(UserManager<ApplicationUser> userManager, NotificationService notificationService)
        {
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var nameClaim = context.Subject.FindAll(JwtClaimTypes.Name);
            context.IssuedClaims.AddRange(nameClaim);

            var roleClaims = context.Subject.FindAll(JwtClaimTypes.Role);
            context.IssuedClaims.AddRange(roleClaims);

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
            }
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
