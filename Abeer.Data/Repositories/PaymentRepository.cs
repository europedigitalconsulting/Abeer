using Abeer.Shared;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class PaymentRepository
    {
        private readonly FunctionalDbContext _context;

        public PaymentRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public  Task<IList<Payment>> GetPayments(Guid transactionId)
        {
            return Task.Run(() => _context.Payments.Where(t => t.TransactionId == transactionId));
        }

        public  Task<Payment> GetPayment(Expression<Func<Payment, bool>> expression)
        {
            return Task.Run(() => _context.Payments.FirstOrDefault(expression));
        }

        public  Task Update(Payment payment)
        {
            return Task.Run(() => _context.Payments.Update(payment));
        }

        public  Task<Payment> Add(Payment payment)
        {
            return Task.Run(() => _context.Payments.Add(payment));
        }

        public Task Remove(Payment payment)
        {
            return Task.Run(() => _context.Payments.Remove(payment.Id));
        }

        public Task<bool> Any(Expression<Func<Payment, bool>> expression)
        {
            return Task.Run(() => _context.Payments.Any(expression));
        }

        public Task<Payment> FirstOrDefault(Expression<Func<Payment, bool>> expression)
        {
            return Task.Run(() => _context.Payments.FirstOrDefault(expression));
        }
    }
}
