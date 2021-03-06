﻿using Abeer.Shared;
using Abeer.Shared.ReferentielTable;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
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
            return Task.Run(() => _context.Contacts.Where(c => c.OwnerId == ownerId && (c.UserAccepted == EnumUserAccepted.ACCEPTED || c.UserAccepted == EnumUserAccepted.PENDING)));
        }

        public Task<Contact> GetContact(Guid id)
        {
            return Task.Run(() => _context.Contacts.FirstOrDefault(c => c.Id == id));
        }
        public Task<Contact> GetContact(string userId, string ownerId)
        {
            return Task.Run(() => _context.Contacts.FirstOrDefault(c => c.UserId == userId && c.OwnerId == ownerId));
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
    }
}
