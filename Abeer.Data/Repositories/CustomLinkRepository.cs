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
        public CustomLinkRepository(FunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }

        public FunctionalDbContext FunctionalDbContext { get; }

        public  Task<IList<CustomLink>> GetCustomLinkLinks(string ownerId) =>
            Task.Run(() => FunctionalDbContext.CustomLinks.Where(u => u.OwnerId == ownerId));

        public  Task<CustomLink> AddCustomLink(CustomLink CustomLink)
        {
            return Task.Run(() => FunctionalDbContext.CustomLinks.Add(CustomLink));
        }

        public  Task<CustomLink> FirstOrDefault(Expression<Func<CustomLink, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.CustomLinks.FirstOrDefault(expression));
        }

        public  Task Remove(CustomLink network)
        {
            return Task.Run(() => FunctionalDbContext.CustomLinks.Remove(network));
        }
    }
}
