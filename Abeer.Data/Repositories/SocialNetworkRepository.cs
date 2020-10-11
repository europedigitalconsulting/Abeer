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
        public SocialNetworkRepository(IFunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }
        public IFunctionalDbContext FunctionalDbContext { get; }

        public async Task<List<SocialNetwork>> GetSocialNetworkLinks(string ownerId) =>
            await FunctionalDbContext.SocialNetworks.Where(u => u.OwnerId == ownerId).ToListAsync();

        public async Task<SocialNetwork> AddSocialNetwork(SocialNetwork socialNetwork)
        {
            await FunctionalDbContext.SocialNetworks.AddAsync(socialNetwork);
            await FunctionalDbContext.SaveChangesAsync();
            return socialNetwork;
        }

        public async Task<SocialNetwork> FirstOrDefaultAsync(Expression<Func<SocialNetwork, bool>> expression)
        {
            return await FunctionalDbContext.SocialNetworks.FirstOrDefaultAsync(expression);
        }

        public async Task Remove(SocialNetwork network)
        {
            FunctionalDbContext.SocialNetworks.Remove(network);
            await FunctionalDbContext.SaveChangesAsync();
        }
    }
}
