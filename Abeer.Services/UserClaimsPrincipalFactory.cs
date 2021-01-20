using System.Security.Claims;
using System.Threading.Tasks;
using Abeer.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Abeer.Services
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public UserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (!string.IsNullOrWhiteSpace(user.FirstName))
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));

            if (!string.IsNullOrWhiteSpace(user.LastName))
                identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

            identity.AddClaim(new Claim("displayname", GetDisplayName(user)));

            identity.AddClaim(new Claim("isonline", user.IsOnline.ToString()));
            identity.AddClaim(new Claim("lastlogin", user.LastLogin.ToString("s")));
            identity.AddClaim(new Claim(ClaimTypes.Country, user.Country ?? "FR"));

            if (user.IsAdmin)
                identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));

            if (user.IsManager)
                identity.AddClaim(new Claim(ClaimTypes.Role, "manager"));

            if (user.IsOperator)
                identity.AddClaim(new Claim(ClaimTypes.Role, "operator"));

            return identity;
        }

        private string GetDisplayName(ApplicationUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.DisplayName))
                return user.DisplayName;
            else if (!string.IsNullOrWhiteSpace(user.LastName))
                return user.FirstName + " " + user.LastName;
            else
                return user.UserName;
        }
    }
}
