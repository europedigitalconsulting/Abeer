using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class TokenBatchRepository
    {
        private readonly IFunctionalDbContext _functionalDbContext;
        private readonly TokenItemRepository _tokenItemRepository;

        public TokenBatchRepository(IFunctionalDbContext functionalDbContext)
        {
            _functionalDbContext = functionalDbContext;
            _tokenItemRepository = new TokenItemRepository(functionalDbContext);
        }

        IIncludableQueryable<TokenBatch, List<TokenBatchStatu>> IncludableQueryables =>
            _functionalDbContext.TokenBatches.Include(b => b.TokenItems)
                            .Include(b => b.TokenBatchStatus);

        public async Task<List<TokenBatch>> GetTokenBatches()
        {
            return await IncludableQueryables.ToListAsync();
        }

        public Task<TokenBatch> FindAsync(long id)
        {
            return IncludableQueryables.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TokenBatchStatu> AddStatusAsync(TokenBatchStatu tokenBatchStatu)
        {
            var entity = await _functionalDbContext.TokenBatchStatus.AddAsync(tokenBatchStatu);
            await _functionalDbContext.SaveChangesAsync();
            return entity.Entity;
        }

        public async void Update(TokenBatch tokenBatch)
        {
            _functionalDbContext.TokenBatches.Update(tokenBatch);
            await _functionalDbContext.SaveChangesAsync();
        }
        static readonly Random rdm = new Random();

        public async Task<TokenBatch> AddAsync(TokenBatch tokenBatch, string userId)
        {
            var entity = await _functionalDbContext.TokenBatches.AddAsync(tokenBatch);
            await _functionalDbContext.SaveChangesAsync();

            tokenBatch = entity.Entity;

            var batchStatu = new TokenBatchStatu
            {
                StatusDate = DateTime.UtcNow,
                Status = TokenBatchStatus.Created,
                TokenBatch = tokenBatch,
                UserId = userId
            };

            await AddStatusAsync(batchStatu);

            for (int i = 0; i < tokenBatch.PartsItemsCount; i++)
            {
                await _tokenItemRepository.AddToken(new TokenItem
                {
                    GeneratedDate = DateTime.UtcNow,
                    IsGenerated = false,
                    IsProcessing = false,
                    IsUsed = false,
                    PartNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100000, 999999)),
                    PartPosition = i,
                    PinCode = KeyGenerator.GeneratePinCode(8).ToString(),
                    TokenBatch= entity.Entity
                });
            }

            return await FirstOrDefaultAsync(b=>b.BatchNumber == tokenBatch.BatchNumber);
        }

        public async void Remove(TokenBatch tokenBatch)
        {
            _functionalDbContext.TokenBatches.Remove(tokenBatch);
            await _functionalDbContext.SaveChangesAsync();
        }

        public Task<bool> AnyAsync(Expression<Func<TokenBatch, bool>> p) => _functionalDbContext.TokenBatches.AnyAsync(p);

        public bool Any(Expression<Func<TokenBatch, bool>> p)
        {
            return AnyAsync(p).Result;
        }

        public Task<TokenBatch> FirstOrDefaultAsync(Expression<Func<TokenBatch, bool>> expression)
        {
            return _functionalDbContext.TokenBatches.Include(b => b.TokenBatchStatus).Include(b => b.TokenItems).FirstOrDefaultAsync(expression);
        }
    }
}
