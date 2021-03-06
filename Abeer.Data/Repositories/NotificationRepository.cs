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
    public class NotificationRepository
    {
        private readonly FunctionalDbContext _context;

        public NotificationRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<IList<Notification>> GetNotifications()
        {
            return Task.Run(() => _context.Notifications.ToList());
        }
        public Task<IList<Notification>> GetNotifications(string userId, bool isDisplayed = false)
        {
            return Task.Run(() => _context.Notifications.Where(c => c.UserId == userId && c.IsDisplayed == isDisplayed));
        }

        public Task<Notification> GetNotification(Guid id)
        {
            return Task.Run(() => _context.Notifications.FirstOrDefault(c => c.Id == id));
        }

        public Task Update(Notification Notification)
        {
            return Task.Run(() => _context.Notifications.Update(Notification));
        }

        public Task<Notification> Add(Notification Notification)
        {
            return Task.Run(() => _context.Notifications.Add(Notification));
        }

        public Task<Notification> FirstOrDefault(Expression<Func<Notification, bool>> p)
        {
            return Task.Run(() => _context.Notifications.FirstOrDefault(p));
        }

        public Task<IList<Notification>> Where(Expression<Func<Notification, bool>> p)
        {
            return Task.Run(() => _context.Notifications.Where(p));
        }

        public void Remove(Notification Notification)
        {
            _context.Notifications.Remove(Notification.Id);
        }

        public Task<bool> Any(Expression<Func<Notification, bool>> p) => Task.Run(() => _context.Notifications.Any(p));

        public Task Delete(Guid id)
        {
            return Task.Run(() => _context.Notifications.Remove(id));
        }
    }
}
