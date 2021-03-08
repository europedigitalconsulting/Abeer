using Abeer.Shared.Functional;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class SubscriptionRepository
    {
        public SubscriptionRepository(FunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }
        public FunctionalDbContext FunctionalDbContext { get; }

        public Task<IList<Subscription>> All() =>
            Task.Run(() => FunctionalDbContext.Subscriptions.ToList());

        public Task<Subscription> Add(Subscription Subscription)
        {
            return Task.Run(() => FunctionalDbContext.Subscriptions.Add(Subscription));
        }
        public Task<Subscription> SubscriptionValid(string userId)
        {
            return Task.Run(() => FunctionalDbContext.Subscriptions.FirstOrDefault(x => x.UserId == userId && x.CreateDate < DateTime.Now && x.End > DateTime.Now && x.Enable));
        }

        public Task<Subscription> FirstOrDefault(System.Linq.Expressions.Expression<Func<Subscription, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.Subscriptions.FirstOrDefault(expression));
        }

        public Task<IList<Subscription>> Where(System.Linq.Expressions.Expression<Func<Subscription, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.Subscriptions.Where(expression));
        }

        public Task Remove(Subscription network)
        {
            return Task.Run(() => FunctionalDbContext.Subscriptions.Remove(network.Id));
        }

        public Task Update(Subscription subscription)
        {
            return Task.Run(() => FunctionalDbContext.Subscriptions.Update(subscription));
        }
    }
}
