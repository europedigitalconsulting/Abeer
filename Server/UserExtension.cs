using Abeer.Server.Data;
using Abeer.Shared;

using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.Server
{
    public static class UserExtension
    {
        public static string NameIdentifier(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static string DisplayName(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue("displayname");
        }
        public static DateTime? SubscriptionStart(this ClaimsPrincipal claimsPrincipal)
        {
            var value = claimsPrincipal.FindFirstValue("subscribeStart");
            DateTime subscribeStart;

            if (DateTime.TryParse(value, out subscribeStart))
                return subscribeStart;
            return null;
        }
        public static DateTime? SubscriptionEnd(this ClaimsPrincipal claimsPrincipal)
        {
            var value = claimsPrincipal.FindFirstValue("subscribeEnd");
            DateTime subscribeEnd;

            if (DateTime.TryParse(value, out subscribeEnd))
                return subscribeEnd;
            return null;
        }

        public static string UserName(this ClaimsPrincipal claimsPrincipal)
        {
            if (!string.IsNullOrWhiteSpace(claimsPrincipal.Identity.Name))
                return claimsPrincipal.Identity.Name;

            return claimsPrincipal.FindFirstValue(ClaimTypes.Name) ?? DisplayName(claimsPrincipal);
        }

        public static Task<ApplicationUser> CheckEncryptedKeys(this ApplicationUser user, SecurityDbContext applicationDbContext)
        {
            try
            {
                bool generated = false;

                if (user.EncryptionIv == null || user.EncryptionIv.Length < 1)
                {
                    generated = true;
                    user.EncryptionIv = KeyGenerator.GetRandomData(128);
                }

                if (user.EncryptionKey == null || user.EncryptionKey.Length < 1)
                {
                    generated = true;
                    user.EncryptionKey = KeyGenerator.GetRandomData(256);
                }

                if (generated)
                    applicationDbContext.Users.Update(user);

                applicationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            return Task.FromResult(user);
        }
    }
}
