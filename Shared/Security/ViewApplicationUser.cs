using Abeer.Shared.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Abeer.Shared
{
    public class ViewApplicationUser
    {
        public ViewApplicationUser()
        {
            CustomLinks = new List<CustomLink>();
            SocialNetworkConnected = new List<SocialNetwork>();
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool DisplayDescription { get; set; } = true;
        public string DescriptionVideo { get; set; }
        public string DescriptionVideoCover { get; set; }
        public string VideoProfileUrl { get; set; }
        public string VideProfileCoverUrl { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsManager { get; set; }
        public bool IsOperator { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int NumberOfView { get; set; }
        public string PhotoUrl { get; set; }
        public string DigitCode { get; set; }
        public int PinCode { get; set; }
        public int AdsCount { get; set; }
        public IList<SocialNetwork> SocialNetworkConnected { get; set; }
        public IList<CustomLink> CustomLinks { get; set; }
        public bool HasSubscriptionValid { get; set; }
        public bool IsReadOnly { get; set; }

        public DateTime? SubscriptionStart { get; set; }
        public DateTime? SubscriptionEnd { get; set; }
        public int NumberOfContacts { get; set; }
        public int NumberOfAds { get; set; }
        public bool IsUnlimited { get; set; }
        public bool IsUltimate { get; set; }
        public bool IsPayable { get; set; }

        public static implicit operator ViewApplicationUser(ApplicationUser user)
        {
            return new ViewApplicationUser()
            {
                Title = user.Title,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                CustomLinks = new List<CustomLink>(),
                DisplayDescription = user.DisplayDescription,
                DescriptionVideo = user.DescriptionVideo,
                DescriptionVideoCover = user.DescriptionVideoCover,
                VideoProfileUrl = user.VideoProfileUrl,
                VideProfileCoverUrl = user.VideProfileCoverUrl,
                Description = user.Description,
                DigitCode = user.PinDigit,
                DisplayName = user.DisplayName,
                Email = user.NormalizedEmail, 
                FirstName = user.FirstName,
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                IsAdmin = user.IsAdmin,
                IsManager = user.IsManager,
                IsOnline = user.IsOnline,
                IsOperator = user.IsOperator,
                LastLogin = user.LastLogin,
                LastName = user.LastName,
                NumberOfView = user.NubmerOfView,
                PhotoUrl = user.PhotoUrl,
                PinCode = user.PinCode,
                SubscriptionStart = user.SubscriptionStartDate,
                SubscriptionEnd = user.SubscriptionEndDate,
                IsReadOnly = user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate <= DateTime.UtcNow
            };
        }

        public static implicit operator ViewApplicationUser(ClaimsPrincipal claimsPrincipal)
        {
            var view = new ViewApplicationUser()
            {
                Title = claimsPrincipal.FindFirstValue(ClaimNames.Title),
                Address = claimsPrincipal.FindFirstValue(ClaimNames.Address),
                City = claimsPrincipal.FindFirstValue(ClaimNames.City),
                Country = claimsPrincipal.FindFirstValue(ClaimNames.Country),
                DisplayDescription = bool.TryParse(claimsPrincipal.FindFirstValue(ClaimNames.DisplayDescription), out var displayDescription) && displayDescription,
                DescriptionVideo = claimsPrincipal.FindFirstValue(ClaimNames.DescriptionVideo),
                DescriptionVideoCover = claimsPrincipal.FindFirstValue(ClaimNames.DescriptionVideoCover),
                VideoProfileUrl = claimsPrincipal.FindFirstValue(ClaimNames.VideoProfileUrl),
                VideProfileCoverUrl = claimsPrincipal.FindFirstValue(ClaimNames.VideProfileCoverUrl),
                Description = claimsPrincipal.FindFirstValue(ClaimNames.Description),
                DigitCode = claimsPrincipal.FindFirstValue(ClaimNames.DigitCode),
                DisplayName = claimsPrincipal.FindFirstValue(ClaimNames.DisplayName),
                Email = claimsPrincipal.FindFirstValue(ClaimNames.Email),
                FirstName = claimsPrincipal.FindFirstValue(ClaimNames.FirstName),
                Id = claimsPrincipal.FindFirstValue(ClaimNames.Id),
                PhoneNumber = claimsPrincipal.FindFirstValue(ClaimNames.PhoneNumber),
                IsAdmin = claimsPrincipal.HasClaim(c => c.Type == ClaimTypes.Role && c.Value.Contains(ClaimNames.IsAdmin)),
                IsManager = claimsPrincipal.HasClaim(c => c.Type == ClaimTypes.Role && c.Value.Contains(ClaimNames.IsManager)),
                IsOnline = bool.TryParse(claimsPrincipal.FindFirstValue(ClaimNames.IsOnline), out var isOnline) && isOnline,
                IsOperator = claimsPrincipal.HasClaim(c => c.Type == ClaimTypes.Role && c.Value.Contains(ClaimNames.IsOperator)),
                LastLogin = DateTime.TryParse(claimsPrincipal.FindFirstValue(ClaimNames.LastLogin), out var lastLogin ) ? lastLogin : DateTime.Now,
                LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname),
                NumberOfView = int.TryParse(claimsPrincipal.FindFirstValue(ClaimNames.NumberOfView), out var numberOfView) ? numberOfView : -1,
                PhotoUrl = claimsPrincipal.FindFirstValue(ClaimNames.PhotoUrl),
                PinCode = int.TryParse(claimsPrincipal.FindFirstValue(ClaimNames.PinCode), out var pinCode) ? pinCode : -1,
                SubscriptionStart = DateTime.TryParse(claimsPrincipal.FindFirstValue(ClaimNames.SubscriptionStart), out var subscriptionStart) ? subscriptionStart : null,
                SubscriptionEnd = DateTime.TryParse(claimsPrincipal.FindFirstValue(ClaimNames.SubscriptionEnd), out var subscriptionEnd) ? subscriptionEnd : null,
                IsUnlimited = bool.TryParse(claimsPrincipal.FindFirstValue(ClaimNames.IsUnlimited), out var isUnlimited) && isUnlimited,
                IsUltimate = claimsPrincipal.HasClaim(c=>c.Type == ClaimNames.Subscription && c.Value.Contains(ClaimNames.Ultimate)),
                IsPayable = claimsPrincipal.HasClaim(c=>c.Type == ClaimNames.IsPayable)
            };

            view.HasSubscriptionValid = view.IsAdmin || view.IsManager || view.IsUnlimited || view.IsUltimate || (claimsPrincipal.HasClaim(c => c.Type == ClaimNames.Subscription)
                && view.SubscriptionEnd.GetValueOrDefault() > DateTime.UtcNow);

            view.IsReadOnly = view.SubscriptionStart.HasValue && view.SubscriptionEnd.HasValue && view.SubscriptionEnd.GetValueOrDefault(DateTime.UtcNow) < DateTime.UtcNow;
            return view;
        }
    }
}
