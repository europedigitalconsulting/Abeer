using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Security
{
    public class OnlySubscribersRequirement : AuthorizationHandler<OnlySubscribersRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OnlySubscribersRequirement requirement)
        {
            if (context.User.HasClaim(ClaimTypes.Role, "admin") || context.User.HasClaim("IsUnlimited", "True"))
            {
                return Task.Run(() => context.Succeed(requirement));
            }
            if (!context.User.HasClaim(c => c.Type == "subscribeEnd"))
            {
                return Task.Run(() => context.Fail());
            }

            DateTimeFormatInfo usDtfi = new CultureInfo("fr-FR", false).DateTimeFormat;
            var subscribeEnd = Convert.ToDateTime(context.User.FindFirst(c => c.Type == "subscribeEnd").Value, usDtfi);

            if (DateTime.UtcNow > subscribeEnd)
            {
                return Task.Run(() => context.Fail());
            }
            return Task.Run(() => context.Succeed(requirement));
        }
    }
}
