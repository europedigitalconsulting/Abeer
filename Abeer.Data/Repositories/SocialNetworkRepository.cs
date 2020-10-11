using Abeer.Shared;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
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
    }
}
