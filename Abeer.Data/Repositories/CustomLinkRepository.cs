using Abeer.Shared;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class CustomLinkRepository
    {
        public CustomLinkRepository(IFunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }

        public IFunctionalDbContext FunctionalDbContext { get; }

        public async Task<List<CustomLink>> GetCustomLinkLinks(string ownerId) =>
            await FunctionalDbContext.CustomLinks.Where(u => u.OwnerId == ownerId).ToListAsync();

        public async Task<CustomLink> AddCustomLink(CustomLink CustomLink)
        {
            await FunctionalDbContext.CustomLinks.AddAsync(CustomLink);
            await FunctionalDbContext.SaveChangesAsync();
            return CustomLink;
        }

        public async Task<CustomLink> FirstOrDefaultAsync(Expression<Func<CustomLink, bool>> expression)
        {
            return await FunctionalDbContext.CustomLinks.FirstOrDefaultAsync(expression);
        }

        public async Task Remove(CustomLink network)
        {
            FunctionalDbContext.CustomLinks.Remove(network);
            await FunctionalDbContext.SaveChangesAsync();
        }
    }
}
