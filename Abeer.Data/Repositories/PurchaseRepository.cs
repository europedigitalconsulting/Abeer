using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class PurchaseRepository
    {
        private readonly IFunctionalDbContext _context;

        public PurchaseRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        IIncludableQueryable<Purchase, List<TransactionStatu>> includableQueryables
            => _context.Purchase.Include(p => p.PurchaseItems)
            .Include(p => p.Payments)
            .Include(p => p.Wallet)
            .Include(p => p.TransactionStatus);

        public System.Threading.Tasks.Task<List<Purchase>> GetPurchases()
        {
            return includableQueryables.ToListAsync();
        }

        public Task<Purchase> FindAsync(Guid id)
        {
            return includableQueryables.FirstOrDefaultAsync(p => p.Id == id);
        }

        public void Update(Purchase purchase)
        {
            _context.Purchase.Update(purchase);
        }

        public async Task<Purchase> AddAsync(Purchase purchase)
        {
            var entity = await _context.Purchase.AddAsync(purchase);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public void Remove(Purchase purchase)
        {
            _context.Purchase.Remove(purchase);
        }

        public async Task<List<Purchase>> Where(Expression<Func<Purchase, bool>> expression)
        {
            return await includableQueryables.Where(expression).ToListAsync();
        }

        public Task<Purchase> FirstOrDefaultAsync(Expression<Func<Purchase, bool>> expression)
        {
            return includableQueryables.FirstOrDefaultAsync(expression);
        }

        public Task<bool> Any(Expression<Func<Purchase, bool>> expression)
        {
            return _context.Purchase.AnyAsync(expression);
        }
    }
}
