using Abeer.Shared.Functional;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class OrganizationRepository
    {
        private readonly FunctionalDbContext _context;

        public OrganizationRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<IList<Organization>> GetOrganizations()
        {
            return Task.Run(() => _context.Organizations.ToList());
        }

        public Task<Organization> GetOrganization(Guid id)
        {
            return Task.Run(() => _context.Organizations.FirstOrDefault(c => c.Id == id));
        }

        public Task Update(Organization Organization)
        {
            return Task.Run(() => _context.Organizations.Update(Organization));
        }

        public Task<Organization> Add(Organization Organization)
        {
            return Task.Run(() => _context.Organizations.Add(Organization));
        }

        public Task<Organization> FirstOrDefault(Expression<Func<Organization, bool>> p)
        {
            return Task.Run(() => _context.Organizations.FirstOrDefault(p));
        }

        public Task<IList<Organization>> Where(Expression<Func<Organization, bool>> p)
        {
            return Task.Run(() => _context.Organizations.Where(p));
        }

        public void Remove(Organization Organization)
        {
            _context.Organizations.Remove(Organization.Id);
        }

        public Task<bool> Any(Expression<Func<Organization, bool>> p) => Task.Run(() => _context.Organizations.Any(p));

        public Task Delete(Guid id)
        {
            return Task.Run(() => _context.Organizations.Remove(id));
        }
    }
}