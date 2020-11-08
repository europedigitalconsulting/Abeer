using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared.Functional
{
    public class Agency
    {
        public Agency()
        {
            CustomLinks = new List<CustomLink>();
            SocialNetworks = new List<SocialNetwork>();
        }

        public IList<SocialNetwork> SocialNetworks { get; set; }
        public IList<CustomLink> CustomLinks { get; set; }

        public string Id { get; set; }
        public string AgencyName { get; set; }
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int NumberOfView { get; set; }
        public string PhotoUrl { get; set; }
        public string MapUrl {get; set;}
    }
}
