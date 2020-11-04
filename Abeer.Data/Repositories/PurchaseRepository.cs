using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class PurchaseRepository
    {
        private readonly FunctionalDbContext _context;

        public PurchaseRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<IList<Purchase>> GetPurchases()
        {
            return Task.Run(() => _context.Purchase.ToList());
        }

        public Task<Purchase> Find(Guid id)
        {
            return Task.Run(() => _context.Purchase.FirstOrDefault(p => p.Id == id));
        }

        public void Update(Purchase purchase)
        {
            _context.Purchase.Update(purchase);
        }

        public  Task<Purchase> Add(Purchase purchase)
        {
            return Task.Run(() => _context.Purchase.Add(purchase));
        }

        public void Remove(Purchase purchase)
        {
            _context.Purchase.Remove(purchase.Id);
        }

        public  Task<IList<Purchase>> Where(Expression<Func<Purchase, bool>> expression)
        {
            return Task.Run(() => _context.Purchase.Where(expression));
        }

        public Task<Purchase> FirstOrDefault(Expression<Func<Purchase, bool>> expression)
        {
            return Task.Run(() => _context.Purchase.FirstOrDefault(expression));
        }

        public Task<bool> Any(Expression<Func<Purchase, bool>> expression)
        {
            return Task.Run(() => _context.Purchase.Any(expression));
        }
    }
}
