using Abeer.Shared.Functional;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class SubscriptionHistoryRepository
    {
        public SubscriptionHistoryRepository(FunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }
        public FunctionalDbContext FunctionalDbContext { get; }

        public Task<IList<SubscriptionHistory>> All() =>
            Task.Run(() => FunctionalDbContext.SubscriptionHistories.ToList());

        public Task<SubscriptionHistory> Add(SubscriptionHistory SubscriptionHistory)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionHistories.Add(SubscriptionHistory));
        }
        public Task<SubscriptionHistory> SubscriptionValid(Guid userId)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionHistories.FirstOrDefault(x => x.UserId == userId && x.Created < DateTime.Now && x.EndSubscription > DateTime.Now && x.Enable));
        }

        public Task<SubscriptionHistory> FirstOrDefault(Expression<Func<SubscriptionHistory, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionHistories.FirstOrDefault(expression));
        }

        public Task<IList<SubscriptionHistory>> Where(Expression<Func<SubscriptionHistory, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionHistories.Where(expression));
        }

        public Task Remove(SubscriptionHistory network)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionHistories.Remove(network.Id));
        }
    }
}
