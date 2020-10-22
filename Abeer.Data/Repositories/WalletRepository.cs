using Abeer.Shared;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class WalletRepository
    {
        private readonly FunctionalDbContext _context;

        public WalletRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<Wallet> FirstOrDefault(Expression<Func<Wallet, bool>> p)
        {
            return Task.Run(()=> _context.Wallets.FirstOrDefault(p));
        }

        public  Task<Wallet> Add(Wallet wallet)
        {
            return Task.Run(() =>
            {
                var entity = _context.Wallets.Add(wallet);
                return entity;
            });
        }
    }
}
