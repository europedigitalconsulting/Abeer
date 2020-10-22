using Abeer.Shared.Functional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class AdRepository
    {
        private readonly FunctionalDbContext _context;
        public AdRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<AdModel> Add(AdModel current)
        {
            return Task.Run(() => _context.Ads.Add(current));
        }

        public Task<IList<AdModel>> GetAllForAUser(string userId)
        {
            return Task.Run(() => _context.Ads.Where(o => o.OwnerId == userId));
        }

        public Task Update(AdModel ad)
        {
            return Task.Run(() => _context.Ads.Update(ad));
        }

        public  Task Delete(Guid id)
        {
            return Task.Run(() => _context.Ads.Remove(id));
        }

        public  Task<IList<AdModel>> GetVisibled()
        {
            return Task.Run(() => _context.Ads.Where(a => a.StartDisplayTime <= DateTime.UtcNow && a.EndDisplayTime >= DateTime.UtcNow
                && a.IsValid));
        }

        public  Task<AdModel> FirstOrDefault(Expression<Func<AdModel, bool>> p)
        {
            return Task.Run(() => _context.Ads.FirstOrDefault(p));
        }

        public  Task<List<AdModel>> All()
        {
            return Task.Run(() => (_context.Ads.ToList())?.OrderByDescending(a => a.CreateDate).ToList());
        }
    }
}
