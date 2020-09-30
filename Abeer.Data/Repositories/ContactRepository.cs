using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class ContactRepository
    {
        private readonly IFunctionalDbContext _context;

        public ContactRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        public System.Threading.Tasks.Task<List<Contact>> GetContacts(string ownerId)
        {
            return _context.Contacts.Where(c => c.OwnerId == ownerId).ToListAsync();
        }

        public async Task<Contact> GetContact(long id)
        {
            return await _context.Contacts.FindAsync(id);
        }

        public Task Update(Contact contact)
        {
            return _context.Update(contact);
        }

        public async Task<Contact> AddAsync(Contact contact)
        {
            var entity =  await _context.Contacts.AddAsync(contact);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public async Task<Contact> FirstOrDefaultAsync(Expression<Func<Contact, bool>> p)
        {
            return await _context.Contacts.FirstOrDefaultAsync(p);
        }

        public Task<List<Contact>> Where(Expression<Func<Contact, bool>> p)
        {
            return _context.Contacts.Where(p).ToListAsync();
        }

        public ValueTask<Contact> FindAsync(long id)
        {
            return _context.Contacts.FindAsync(id);
        }

        public void Remove(Contact contact)
        {
            _context.Contacts.Remove(contact);
        }

        public Task<bool> AnyAsync(Expression<Func<Contact, bool>> p) => _context.Contacts.AnyAsync(p);

        public bool Any(Expression<Func<Contact, bool>> p)
        {
            return AnyAsync(p).Result;
        }
    }
}
