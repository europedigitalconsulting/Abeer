using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Functional
{
    public class Team
    {
        public Team()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool DisplayDescription { get; set; } = true;
        public string DescriptionVideo { get; set; }
        public string DescriptionVideoCover { get; set; }
        public string VideoProfileUrl { get; set; }
        public string VideProfileCoverUrl { get; set; }
        public string Country { get; set; } = "France";
        public string City { get; set; } = "Palaiseau";
        public string Address { get; set; }
        public int NubmerOfView { get; set; }
        public string LogoUrl { get; set; }
        public Guid OrganizationId { get; set; }
        public string ManagerId { get; set; }
        public string CreatorId { get; set; }
    }
}
