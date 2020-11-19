using Abeer.Shared.Functional;
using System;
using System.Collections.Generic;
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

        public  Task<PaymentModel> Add(PaymentModel current)
        {
            return Task.Run(() => _context.Payments.Add(current));
        }

        public  Task Update(PaymentModel ad)
        {
            return Task.Run(() => _context.Payments.Update(ad));
        }

        public  Task Delete(Guid id)
        {
            return Task.Run(() => _context.Payments.FirstOrDefault(a => a.Id == id));
        }

        public Task<PaymentModel> FirstOrDefault(Expression<Func<PaymentModel, bool>> p)
        {
            return Task.Run(() => _context.Payments.FirstOrDefault(p));
        }
        public  Task<IList<PaymentModel>> All()
        {
            return Task.Run(() => _context.Payments.ToList());
        }
    }
}
