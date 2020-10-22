using Abeer.Shared;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class SocialNetworkRepository
    {
        public SocialNetworkRepository(FunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }
        public FunctionalDbContext FunctionalDbContext { get; }

        public  Task<IList<SocialNetwork>> GetSocialNetworkLinks(string ownerId) =>
            Task.Run(() => FunctionalDbContext.SocialNetworks.Where(u => u.OwnerId == ownerId));

        public  Task<SocialNetwork> AddSocialNetwork(SocialNetwork socialNetwork)
        {
            return Task.Run(() => FunctionalDbContext.SocialNetworks.Add(socialNetwork));
        }

        public  Task<SocialNetwork> FirstOrDefault(Expression<Func<SocialNetwork, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.SocialNetworks.FirstOrDefault(expression));
        }

        public  Task Remove(SocialNetwork network)
        {
            return Task.Run(() => FunctionalDbContext.SocialNetworks.Remove(network));
        }
    }
}
