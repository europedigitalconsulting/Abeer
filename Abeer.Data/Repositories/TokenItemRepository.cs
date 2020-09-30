using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class TokenItemRepository
    {
        private readonly IFunctionalDbContext _context;

        public TokenItemRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        public Task<int> CountAll()
        {
            return _context.TokenItems.CountAsync();
        }

        public Task<int> CountUsed()
        {
            return _context.TokenItems.CountAsync(t => t.IsUsed);
        }

        public IQueryable<TokenItem> Where(Expression<Func<TokenItem, bool>> p)
        {
            return _context.TokenItems.Include(t => t.TokenBatch).Where(p);
        }

        public Task<TokenItem> FirstOrDefaultAsync(Expression<Func<TokenItem, bool>> p)
        {
            return _context.TokenItems.Include(t => t.TokenBatch).FirstOrDefaultAsync(p);
        }

        public async Task Update(TokenItem token)
        {
            await _context.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task<TokenItem> AddToken(TokenItem tokenItem)
        {
            var entity = await _context.TokenItems.AddAsync(tokenItem);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }
    }
}
