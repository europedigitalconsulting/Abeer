using Abeer.Shared.Functional;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class SubscriptionPackRepository
    {
        public SubscriptionPackRepository(FunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }
        public FunctionalDbContext FunctionalDbContext { get; }

        public Task<IList<SubscriptionPack>> All() =>
            Task.Run(() => FunctionalDbContext.SubscriptionPacks.ToList());

        public Task<SubscriptionPack> AddSubscriptionPack(SubscriptionPack SubscriptionPack)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionPacks.Add(SubscriptionPack));
        }

        public Task<SubscriptionPack> FirstOrDefault(Expression<Func<SubscriptionPack, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionPacks.FirstOrDefault(expression));
        }

        public Task<IList<SubscriptionPack>> Where(Expression<Func<SubscriptionPack, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionPacks.Where(expression));
        }

        public Task Remove(SubscriptionPack network)
        {
            return Task.Run(() => FunctionalDbContext.SubscriptionPacks.Remove(network.Id));
        }
    }
}
