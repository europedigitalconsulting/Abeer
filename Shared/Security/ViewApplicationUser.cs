using System;
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
        public int NubmerOfView { get; set; }
        public string PhotoUrl { get; set; }
        public string Pincode { get; set; }
        public string DigitCode { get; set; }
        public IList<SocialNetwork> SocialNetworkConnected { get; set; }
        public IList<CustomLink> CustomLinks { get; set; }
    }
}
