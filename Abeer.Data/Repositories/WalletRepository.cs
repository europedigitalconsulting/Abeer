using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class WalletRepository
    {
        private readonly IFunctionalDbContext _context;

        public WalletRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        public Task<Wallet> FirstOrDefaultAsync(Expression<Func<Wallet, bool>> p)
        {
            return _context.Wallets.FirstOrDefaultAsync(p);
        }

        public async Task<Wallet> AddAsync(Wallet wallet)
        {
            var entity = await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }
    }
}
