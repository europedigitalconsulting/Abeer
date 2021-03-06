﻿using System;
using System.Collections.Generic;

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
        public bool IsReadOnly { get; set; }

        public DateTime? SubscriptionStart { get; set; }
        public DateTime? SubscriptionEnd { get; set; }
        public int NumberOfContacts { get; set; }


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
    }
}
