using Abeer.Shared;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class TokenItemRepository
    {
        private readonly FunctionalDbContext _context;

        public TokenItemRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<int> CountAll()
        {
            return Task.Run(()=> _context.TokenItems.Count());
        }

        public Task<int> CountUsed()
        {
            return Task.Run(() => _context.TokenItems.Count(t => t.IsUsed));
        }

        public IList<TokenItem> Where(Expression<Func<TokenItem, bool>> p)
        {
            return _context.TokenItems.Where(p);
        }

        public Task<TokenItem> FirstOrDefault(Expression<Func<TokenItem, bool>> p)
        {
            return Task.Run(() => _context.TokenItems.FirstOrDefault(p));
        }

        public  Task Update(TokenItem token)
        {
            return Task.Run(() => _context.TokenItems.Update(token));
        }

        public  Task<TokenItem> AddToken(TokenItem tokenItem)
        {
            return Task.Run(() =>
            {
                var entity = _context.TokenItems.Add(tokenItem);
                return entity;
            });
        }
    }
}
