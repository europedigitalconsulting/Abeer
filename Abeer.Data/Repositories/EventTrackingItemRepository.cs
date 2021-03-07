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
    public class EventTrackingItemRepository
    {
        private readonly FunctionalDbContext _context;

        public EventTrackingItemRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<IList<EventTrackingItem>> GetEventTrackingItems()
        {
            return Task.Run<IList<EventTrackingItem>>(() => _context.EventTrackingItems.ToList().OrderByDescending(n=>n.CreatedDate).ToList());
        }

        public Task<IList<EventTrackingItem>> GetEventTrackingItems(string userId, string EventTrackingItemType)
        {
            return Task.Run<IList<EventTrackingItem>>(() => _context.EventTrackingItems.Where(n=>n.UserId == userId && n.Key == EventTrackingItemType)
            .OrderByDescending(n => n.CreatedDate).ToList());
        }

        public Task<EventTrackingItem> GetEventTrackingItem(Guid id)
        {
            return Task.Run(() => _context.EventTrackingItems.FirstOrDefault(c => c.Id == id));
        }

        public Task Update(EventTrackingItem EventTrackingItem)
        {
            return Task.Run(() => _context.EventTrackingItems.Update(EventTrackingItem));
        }

        public Task<IList<EventTrackingItem>> GetEventTrackingItems(string userId)
        {
            return Task.Run<IList<EventTrackingItem>>(() => _context.EventTrackingItems.Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate).ToList());
        }

        public Task<EventTrackingItem> Add(EventTrackingItem EventTrackingItem)
        {
            return Task.Run(() => _context.EventTrackingItems.Add(EventTrackingItem));
        }

        public Task<EventTrackingItem> FirstOrDefault(Expression<Func<EventTrackingItem, bool>> p)
        {
            return Task.Run(() => _context.EventTrackingItems.FirstOrDefault(p));
        }

        public Task<IList<EventTrackingItem>> Where(Expression<Func<EventTrackingItem, bool>> p)
        {
            return Task.Run(() => _context.EventTrackingItems.Where(p));
        }

        public void Remove(EventTrackingItem EventTrackingItem)
        {
            _context.EventTrackingItems.Remove(EventTrackingItem.Id);
        }

        public Task<bool> Any(Expression<Func<EventTrackingItem, bool>> p) => Task.Run(() => _context.EventTrackingItems.Any(p));

        public Task Delete(Guid id)
        {
            return Task.Run(() => _context.EventTrackingItems.Remove(id));
        }
    }
}
