using Abeer.Shared.Functional;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class AdPriceRepository
    {
        private readonly FunctionalDbContext _context;
        public AdPriceRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public  Task<AdPrice> Add(AdPrice current)
        {
            return Task.Run(() => _context.AdPrices.Add(current));
        }

        public  Task Update(AdPrice ad)
        {
            return Task.Run(() => _context.AdPrices.Update(ad));
        }

        public  Task Delete(Guid id)
        {
            return Task.Run(() => _context.AdPrices.FirstOrDefault(a => a.Id == id));
        }

        public Task<AdPrice> FirstOrDefault(Expression<Func<AdPrice, bool>> p)
        {
            return Task.Run(() => _context.AdPrices.FirstOrDefault(p));
        }
        public  Task<IList<AdPrice>> All()
        {
            return Task.Run(() => _context.AdPrices.ToList());
        }
    }
}
