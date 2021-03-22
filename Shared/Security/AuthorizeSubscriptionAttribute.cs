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
            if (context.User.Identity.IsAuthenticated)
            {
                var user = (ViewApplicationUser)context.User;

                if (user.IsAdmin || user.IsManager || user.IsUnlimited)
                {
                    return Task.Run(() => context.Succeed(requirement));
                }

                else if (user.SubscriptionEnd.GetValueOrDefault(DateTime.UtcNow) >= DateTime.UtcNow)
                {
                    return Task.Run(() => context.Succeed(requirement));
                }
            }

            return Task.Run(() => context.Fail());
        }
    }
}
