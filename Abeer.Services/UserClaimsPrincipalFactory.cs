using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Abeer.Data.UnitOfworks;
using Abeer.Shared;
using Abeer.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Abeer.Services
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        private FunctionalUnitOfWork _functionalUnitOfWork;

        public UserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor, FunctionalUnitOfWork functionalUnitOfWork) : base(userManager, optionsAccessor)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (!user.IsAdmin && !user.IsUnlimited)
            {
                var subscription = await _functionalUnitOfWork.SubscriptionRepository.GetLatestSubscriptionForUser(user.Id);

                if (subscription != null && subscription.SubscriptionPackId != Guid.Empty)
                {
                    var pack = await _functionalUnitOfWork.SubscriptionPackRepository.FirstOrDefault(s => s.Id == subscription.SubscriptionPackId);

                    if (subscription != null)
                    {
                        identity.AddClaim(ClaimNames.Subscription, pack.Label.ToLower());
                        identity.AddClaim(ClaimNames.IsPayable, pack.Price > 0 ? "true" : "false");
                    }
                }
            }


            return identity;
        }
    }
}
