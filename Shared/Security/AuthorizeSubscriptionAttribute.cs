using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Security
{
    public class OnlySubscribersRequirement : AuthorizationHandler<OnlySubscribersRequirement>, IAuthorizationRequirement
    {

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OnlySubscribersRequirement requirement)
        {
            if (context.User.HasClaim(ClaimTypes.Role, "admin"))
            {
                return Task.Run(() => context.Succeed(requirement));
            }
            if (!context.User.HasClaim(c => c.Type == "subscribeEnd"))
            {
                return Task.Run(() => context.Fail());
            }

            var subscribeEnd = Convert.ToDateTime(context.User.FindFirst(c => c.Type == "subscribeEnd").Value);

            if (DateTime.Today.AddDays(5) > subscribeEnd)
            {
                return Task.Run(() => context.Fail());
            }
            return Task.Run(() => context.Succeed(requirement));
        }
    }
}
