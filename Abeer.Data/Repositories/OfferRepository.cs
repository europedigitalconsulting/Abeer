using Abeer.Shared.Functional;

using System;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class OfferRepository
    {
        private readonly IFunctionalDbContext _context;
        public OfferRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        public async Task<OfferModel> AddAsync(OfferModel current)
        {
            var entity = await _context.Offers.AddAsync(current);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }
    }
}
