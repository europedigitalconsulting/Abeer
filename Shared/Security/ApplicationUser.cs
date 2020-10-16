﻿using Microsoft.AspNetCore.Identity;

using System;

namespace Abeer.Shared
{
    public class ApplicationUser : IdentityUser
    {
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
        public string Country { get; set; }
        public string City { get; set; }
        public string PinCode { get; set; }
        public int PinDigit { get; set; }
        public byte[] EncryptionIv { get; set; } //Iv = Initialization vector
        public byte[] EncryptionKey { get; set; }
        public string Address { get; set; }
        public int NubmerOfView { get; set; }
        public string PhotoUrl { get; set; }
    }
}
