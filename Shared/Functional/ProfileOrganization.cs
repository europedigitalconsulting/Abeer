using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abeer.Shared.Functional
{
    public class ProfileOrganization
    {
        [Key]
        public Guid Id { get; set; }
        public string ContactId { get; set; } 
        public string ManagerId { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid TeamId { get; set; }
    }

    public class ProfileOrganizationViewModel
    {
        private ProfileOrganization profileOrganization;

        public ProfileOrganizationViewModel()
        {

        }

        public ProfileOrganizationViewModel(ProfileOrganization profileOrganization)
        {
            this.profileOrganization = profileOrganization;

            Id = profileOrganization.Id;
            ContactId = profileOrganization.ContactId;
            ManagerId = profileOrganization.ManagerId;
            OrganizationId = profileOrganization.OrganizationId;
            TeamId = profileOrganization.TeamId;
        }

        public Guid Id { get; set; }
        public string ContactId { get; set; }
        public string ManagerId { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid TeamId { get; set; }
        public Organization Organization { get; set; }
        public Team Team { get; set; }
        public ViewApplicationUser Manager { get; set; }

        public void SetOrganization(Organization organization)
        {
            Organization = organization;
            OrganizationId = organization.Id;
        }

        public static implicit operator ProfileOrganization(ProfileOrganizationViewModel vm)
        {
            return new ProfileOrganization
            {
                Id = vm.Id,
                ContactId = vm.ContactId,
                ManagerId = vm.ManagerId,
                OrganizationId = vm.OrganizationId,
                TeamId = vm.TeamId
            };
        }

        public static implicit operator ProfileOrganizationViewModel(ProfileOrganization profileOrganization)
        {
            return new ProfileOrganizationViewModel(profileOrganization);
        }

        public void SetTeam(Team team)
        {
            Team = team;
            TeamId = team.Id;
        }

        public void SetManager(ViewApplicationUser manager)
        {
            Manager = manager;
            ManagerId = manager.Id;
        }
    }
}
