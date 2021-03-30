using Abeer.Shared.Functional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Abeer.Data.Repositories
{
    public class QrCodeRepository
    {
        private readonly FunctionalDbContext _context;
        public QrCodeRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<QrCode> Add(QrCode current)
        {
            return Task.Run(() => _context.QrCodes.Add(current));
        }

    
        public Task<IList<QrCode>> Get(Guid ownerId)
        {
            return Task.Run(() => _context.QrCodes.Where(x => x.OwnerId == ownerId));
        }
        public  Task Delete(Guid id)
        {
            return Task.Run(() => _context.QrCodes.Remove(id));
        }

        public Task<IList<QrCode>> Where(Expression<Func<QrCode, bool>> filter)
        {
            return Task.Run(() => _context.QrCodes.Where(filter));
        }
    }
}
