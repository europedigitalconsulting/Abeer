using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ReferentielTable;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class InvitationRepository
    {
        private readonly FunctionalDbContext _context;

        public InvitationRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<IList<Invitation>> GetInvitations()
        {
            return Task.Run<IList<Invitation>>(() => _context.Invitations.ToList().OrderByDescending(n=>n.CreatedDate).ToList());
        }

        public Task<IList<Invitation>> GetInvitationsBy(string userId)
        {
            return Task.Run<IList<Invitation>>(() => _context.Invitations.Where(i => i.OwnedId == userId)
                .OrderByDescending(n => n.CreatedDate).ToList());
        }

        public Task<IList<Invitation>> GetInvitationsFor(string userId)
        {
            return Task.Run<IList<Invitation>>(() => _context.Invitations.Where(i => i.ContactId == userId)
                .OrderByDescending(n => n.CreatedDate).ToList());
        }

        public Task<IList<Invitation>> GetInvitations(string userId, int invitationStatus)
        {
            return Task.Run<IList<Invitation>>(() => _context.Invitations.Where(i=>i.OwnedId == userId && i.InvitationStat == invitationStatus)
                .OrderByDescending(n => n.CreatedDate).ToList());
        }

        public Task<Invitation> GetInvitation(Guid id)
        {
            return Task.Run(() => _context.Invitations.FirstOrDefault(c => c.Id == id));
        }

        public Task<Invitation> GetInvitation(string ownedId, string contactId)
        {
            return Task.Run(() => _context.Invitations.FirstOrDefault(c => c.OwnedId == ownedId && c.ContactId == contactId));
        }


        public Task Update(Invitation Invitation)
        {
            return Task.Run(() => _context.Invitations.Update(Invitation));
        }

        public Task<Invitation> Add(Invitation Invitation)
        {
            return Task.Run(() => _context.Invitations.Add(Invitation));
        }

        public Task<Invitation> FirstOrDefault(Expression<Func<Invitation, bool>> p)
        {
            return Task.Run(() => _context.Invitations.FirstOrDefault(p));
        }

        public Task<IList<Invitation>> Where(Expression<Func<Invitation, bool>> p)
        {
            return Task.Run(() => _context.Invitations.Where(p));
        }

        public void Remove(Invitation Invitation)
        {
            _context.Invitations.Remove(Invitation.Id);
        }

        public Task<bool> Any(Expression<Func<Invitation, bool>> p) => Task.Run(() => _context.Invitations.Any(p));

        public Task Delete(Guid id)
        {
            return Task.Run(() => _context.Invitations.Remove(id));
        }
    }
}
