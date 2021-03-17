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
    public class MessageRepository
    {
        private readonly FunctionalDbContext _context;

        public MessageRepository(FunctionalDbContext context)
        {
            _context = context;
        }         

        public Task<IList<Message>> GetMessages(Guid userId, Guid contactId)
        {
            return Task.Run<IList<Message>>(() => _context.Messages.Where(n => (n.UserIdFrom == userId && n.UserIdTo == contactId) || (n.UserIdFrom == contactId && n.UserIdTo == userId)).ToList());
        }
        public Task<IList<Message>> GetMessageUnread(Guid userIdTo)
        {
            var resp = _context.Messages.Where(x => x.UserIdTo == userIdTo && x.DateReceived == null).ToList();

            return Task.Run<IList<Message>>(() => resp);
        }

        public Task Update(Message Message)
        {
            return Task.Run(() => _context.Messages.Update(Message));
        }

        public Task<Message> Add(Message Message)
        {
            return Task.Run(() => _context.Messages.Add(Message));
        }

        public Task<Message> FirstOrDefault(Expression<Func<Message, bool>> p)
        {
            return Task.Run(() => _context.Messages.FirstOrDefault(p));
        }

        public Task<IList<Message>> Where(Expression<Func<Message, bool>> p)
        {
            return Task.Run(() => _context.Messages.Where(p));
        }

        public void Remove(Message Message)
        {
            _context.Messages.Remove(Message.Id);
        }

        public Task<bool> Any(Expression<Func<Message, bool>> p) => Task.Run(() => _context.Messages.Any(p));

        public Task Delete(Guid id)
        {
            return Task.Run(() => _context.Messages.Remove(id));
        }
    }
}
