using IdentityModel;
using System;
using System.Security.Claims;

namespace Abeer.Shared.Security
{
    public static class ClaimWriterExtension
    {
        public static void WriteClaims(this ApplicationUser user, ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim(JwtClaimTypes.Subject, user.Id));
            identity.AddClaim(new Claim(JwtClaimTypes.Name, user.UserName));

            AddClaim(identity, ClaimNames.Title, user.Title);
            AddClaim(identity, ClaimNames.Address, user.Address);
            AddClaim(identity, ClaimNames.City, user.City);
            AddClaim(identity, ClaimNames.Country, user.Country);
            AddClaim(identity, ClaimNames.DisplayDescription, user.DisplayDescription);
            AddClaim(identity, ClaimNames.DescriptionVideo, user.DescriptionVideo);
            AddClaim(identity, ClaimNames.DescriptionVideoCover, user.DescriptionVideoCover);
            AddClaim(identity, ClaimNames.VideoProfileUrl, user.VideoProfileUrl);
            AddClaim(identity, ClaimNames.VideProfileCoverUrl, user.VideProfileCoverUrl);
            AddClaim(identity, ClaimNames.Description, user.Description);
            AddClaim(identity, ClaimNames.DigitCode, user.PinCode);
            AddClaim(identity, ClaimNames.DisplayName, GetDisplayName(user));
            AddClaim(identity, ClaimNames.Email, user.Email);
            AddClaim(identity, ClaimNames.FirstName, user.FirstName);
            AddClaim(identity, ClaimNames.Id, user.Id);
            AddClaim(identity, ClaimNames.PhoneNumber, user.PhoneNumber);
            AddClaim(identity, ClaimNames.IsUnlimited, user.IsUnlimited);

            AddRole(identity, ClaimNames.IsAdmin, user.IsAdmin);
            AddRole(identity, ClaimNames.IsManager, user.IsManager);
            AddRole(identity, ClaimNames.IsOperator, user.IsOperator);
            AddRole(identity, ClaimNames.IsOnline, user.IsOnline);

            AddClaim(identity, ClaimNames.LastLogin, user.LastLogin);
            AddClaim(identity, ClaimNames.LastName, user.LastName);
            AddClaim(identity, ClaimNames.NumberOfView, user.NubmerOfView);
            AddClaim(identity, ClaimNames.PhotoUrl, user.PhotoUrl);
            AddClaim(identity, ClaimNames.PinCode, user.PinCode);
            AddClaim(identity, ClaimNames.SubscriptionStart, user.SubscriptionStartDate);
            AddClaim(identity, ClaimNames.SubscriptionEnd, user.SubscriptionEndDate);
            AddClaim(identity, ClaimNames.IsReadOnly, user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value <= DateTime.UtcNow);
        }

        public static void AddRole(ClaimsIdentity identity, string name, bool isAllowed)
        {
            if (!isAllowed)
                return;

            if (identity.HasClaim(ClaimTypes.Role, name) == false)
                identity.AddClaim(new Claim(ClaimTypes.Role, name));
        }

        public static void AddClaim(this ClaimsIdentity identity, string name, object value)
        {
            if (!identity.HasClaim(c => c.Type == name) && value != null)
                identity.AddClaim(new Claim(name, value.ToString()));
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
    }
}
