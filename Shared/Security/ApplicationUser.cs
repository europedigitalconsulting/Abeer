using Microsoft.AspNetCore.Identity;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abeer.Shared
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = "MArc";
        public string LastName { get; set; } = "Absalon";
        public string DisplayName { get; set; } = "Over";
        public string Title { get; set; } 
        public string Description { get; set; }
        public bool IsOnline { get; set; }
        [NotMapped]
        public bool IsLocked { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsManager { get; set; }
        public bool IsOperator { get; set; }
        public string Country { get; set; } = "France";
        public string City { get; set; } = "Palaiseau";
        public string PinCode { get; set; }
        public int PinDigit { get; set; } = 12345;
        public byte[] EncryptionIv { get; set; } //Iv = Initialization vector
        public byte[] EncryptionKey { get; set; }
        public string Address { get; set; }
        public int NubmerOfView { get; set; }
        public string PhotoUrl { get; set; }
    }
}
