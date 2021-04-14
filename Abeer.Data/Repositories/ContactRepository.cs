using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ReferentielTable;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class ContactRepository
    {
        private readonly FunctionalDbContext _context;

        public ContactRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<IList<Contact>> GetContacts()
        {
            return Task.Run(() => _context.Contacts.ToList());
        }
        public Task<IList<Contact>> GetContacts(string ownerId)
        {
            return Task.Run(() => _context.Contacts.Where(c => c.OwnerId == ownerId && c.UserAccepted == EnumUserAccepted.ACCEPTED));
        }

        public Task<Contact> GetContact(Guid id)
        {
            return Task.Run(() => _context.Contacts.FirstOrDefault(c => c.Id == id));
        }

        public Task<Contact> GetContact(string ownerId, string contactId)
        {
            return Task.Run(() => _context.Contacts.FirstOrDefault(c => c.OwnerId == ownerId && c.UserId == contactId));
        }

        public Task Update(Contact contact)
        {
            return Task.Run(() => _context.Contacts.Update(contact));
        }

        public Task<Contact> Add(Contact contact)
        {
            return Task.Run(() => _context.Contacts.Add(contact));
        }

        public Task<Contact> FirstOrDefault(Expression<Func<Contact, bool>> p)
        {
            return Task.Run(() => _context.Contacts.FirstOrDefault(p));
        }

        public Task<IList<Contact>> Where(Expression<Func<Contact, bool>> p)
        {
            return Task.Run(() => _context.Contacts.Where(p));
        }

        public void Remove(Contact contact)
        {
            _context.Contacts.Remove(contact.Id);
        }

        public Task<bool> Any(Expression<Func<Contact, bool>> p) => Task.Run(() => _context.Contacts.Any(p));

        public Task Delete(Guid id)
        {
            return Task.Run(() => _context.Contacts.Remove(id));
        }

        public Task<IList<Contact>> GetContactRequests(string userId)
        {
            return Task.Run(() => _context.Contacts.Where(c => c.UserId == userId && (c.UserAccepted == EnumUserAccepted.PENDING)));
        }

        public Task<ProfileOrganizationViewModel> GetOrganization(string contactId)
        {
            return Task.Run(() =>
            {

                var o = _context.ProfileOrganizations.FirstOrDefault(c => c.ContactId == contactId);

                if (o != null)
                {
                    var vm = new ProfileOrganizationViewModel(o);
                    vm.Organization = _context.Organizations.FirstOrDefault(o => o.Id == vm.OrganizationId);
                    vm.Team = _context.Teams.FirstOrDefault(t => t.Id == vm.TeamId);
                    return vm;
                }
                else
                    return default;
            });
        }

        public Task<IList<ProfileOrganization>> GetProfilesByTeam(Guid organizationId, Guid teamId)
        {
            return Task.Run(() =>
            {
                return _context.ProfileOrganizations.Where(o => o.OrganizationId == organizationId && o.TeamId == teamId);
            });
        }

        public Task AddOrganization(ProfileOrganizationViewModel profileOrganizationViewModel)
        {
            return Task.Run(() =>
            {
                _context.ProfileOrganizations.Add(profileOrganizationViewModel);
            });
        }

        public Task UpdateOrganization(ProfileOrganizationViewModel profileOrganizationViewModel)
        {
            return Task.Run(() =>
            {
                var current = _context.ProfileOrganizations.FirstOrDefault(o => o.Id == profileOrganizationViewModel.Id);

                current.ManagerId = profileOrganizationViewModel.ManagerId;
                current.OrganizationId = profileOrganizationViewModel.OrganizationId;
                current.TeamId = profileOrganizationViewModel.TeamId;

                _context.SaveChange();
            });
        }

        public Task<IList<ProfileOrganization>> GetProfilesByOrganization(Guid organizationId)
        {
            return Task.Run(() =>
            {
                return _context.ProfileOrganizations.Where(o => o.OrganizationId == organizationId);
            });
        }
    }
}