using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Abeer.Shared
{
    public class ViewContact
    {
        private ApplicationUser user;

        public ViewContact()
        {
            CustomLinks = new List<CustomLink>();
            SocialNetworks = new List<SocialNetwork>();
        }

        public ViewContact(ApplicationUser user, Contact contact):this()
        {
            Id = contact.Id;
            UserId = contact.UserId;
            OwnerId = contact.OwnerId;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DisplayName = user.DisplayName;
            Title = user.Title;
            Description = user.Description;
            IsOnline = user.IsOnline;
            LastLogin = user.LastLogin;
            Address = user.Address;
            Country = user.Country;
            City = user.City;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            PhotoUrl = string.IsNullOrWhiteSpace(user.PhotoUrl) ? user.GravatarUrl() : user.PhotoUrl;
        }

        public List<SocialNetwork> SocialNetworks { get; set; }
        public List<CustomLink> CustomLinks { get; set; }

        public long Id { get; set; }
        public string UserId { get; set; }
        public string OwnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastLogin { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int NumberOfView { get; set; }
        public string PhotoUrl { get; set; }
    }
    public class Contact
    {
        [Key]
        public long Id { get; set; }
        public string UserId { get; set; }
        public string OwnerId { get; set; }
    }
}
