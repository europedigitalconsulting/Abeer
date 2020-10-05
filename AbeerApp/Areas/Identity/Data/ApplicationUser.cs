using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Column(TypeName ="nvarchar(100)")]
        public string FirstName { get; set; }
        
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string LastName { get; set; }
        
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string DisplayName { get; set; }
        
        [PersonalData]
        public DateTime LastLogin { get; set; }
        
        [PersonalData]
        public bool IsAdmin { get; set; }
        
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string PathImage { get; set; }

    }
}
