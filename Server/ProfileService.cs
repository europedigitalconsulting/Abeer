using Abeer.Shared;

using IdentityModel;

using IdentityServer4.Models;
using IdentityServer4.Services;

using Microsoft.AspNetCore.Identity;

using System;
using System.Linq;

using System.Threading.Tasks;

namespace Abeer.Server
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
            }

            if (!context.IssuedClaims.Any(c => c.Type == "photoUrl"))
                context.IssuedClaims.Add(new System.Security.Claims.Claim("photoUrl", (string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl)));
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
