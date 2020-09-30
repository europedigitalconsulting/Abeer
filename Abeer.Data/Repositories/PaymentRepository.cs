using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class PaymentRepository
    {
        private readonly IFunctionalDbContext _context;

        public PaymentRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payment>> GetPayments(Guid transactionId)
        {
            return await _context.Payment.Where(t => t.TransactionId == transactionId).ToListAsync();
        }

        public async Task<Payment> GetPayment(Expression<Func<Payment, bool>> expression)
        {
            return await _context.Payment.FirstOrDefaultAsync(expression);
        }

        public async Task Update(Payment payment)
        {
            await _context.Update(payment);
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            var entity = await _context.Payment.AddAsync(payment);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public Task Remove(Payment payment)
        {
            _context.Payment.Remove(payment);
            return Task.CompletedTask;
        }

        public Task<bool> AnyAsync(Expression<Func<Payment, bool>> expression)
        {
            return _context.Payment.AnyAsync(expression);
        }

        public Task<Payment> FirstOrDefaultAsync(Expression<Func<Payment, bool>> expression)
        {
            return _context.Payment.Include(p => p.Transaction).ThenInclude(t => t.TransactionStatus).FirstOrDefaultAsync(expression);
        }

        public bool Any(Expression<Func<Payment, bool>> expression)
        {
            return AnyAsync(expression).Result;
        }
    }
}
