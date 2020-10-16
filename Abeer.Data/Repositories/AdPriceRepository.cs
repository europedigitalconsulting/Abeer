using Abeer.Shared.Functional;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class AdPriceRepository
    {
        private readonly IFunctionalDbContext _context;
        public AdPriceRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        public async Task<AdPrice> AddAsync(AdPrice current)
        {
            var entity = await _context.AdPrices.AddAsync(current);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public async Task Update(AdPrice ad)
        {
            _context.AdPrices.Update(ad);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var ad = await _context.AdPrices.FirstOrDefaultAsync(a => a.Id == id);
            _context.AdPrices.Remove(ad);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AdPrice>> AllAsync()
        {
            return await _context.AdPrices.ToListAsync();
        }
    }
}
