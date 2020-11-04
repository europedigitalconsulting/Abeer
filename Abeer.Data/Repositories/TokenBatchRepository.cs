using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class TokenBatchRepository
    {
        private readonly FunctionalDbContext _functionalDbContext;
        private readonly TokenItemRepository _tokenItemRepository;

        public TokenBatchRepository(FunctionalDbContext functionalDbContext)
        {
            _functionalDbContext = functionalDbContext;
            _tokenItemRepository = new TokenItemRepository(functionalDbContext);
        }

        public Task<IList<TokenBatch>> GetTokenBatches()
        {
            return Task.Run(() => _functionalDbContext.TokenBatches.ToList());
        }

        public Task<TokenBatch> Find(long id)
        {
            return Task.Run(() => _functionalDbContext.TokenBatches.FirstOrDefault(t => t.Id == id));
        }

        public  Task<TokenBatchStatu> AddStatus(TokenBatchStatu tokenBatchStatu)
        {
            return Task.Run(() =>
            {
                var entity = _functionalDbContext.TokenBatchStatus.Add(tokenBatchStatu);
                return entity;
            });
        }

        public  void Update(TokenBatch tokenBatch)
        {
            Task.Run(() => _functionalDbContext.TokenBatches.Update(tokenBatch));
        }

        static readonly Random rdm = new Random();

        public  Task<TokenBatch> Add(TokenBatch tokenBatch, string userId)
        {
            tokenBatch = _functionalDbContext.TokenBatches.Add(tokenBatch);

            var batchStatu = new TokenBatchStatu
            {
                StatusDate = DateTime.UtcNow,
                Status = TokenBatchStatus.Created,
                TokenBatch = tokenBatch,
                UserId = userId
            };

            AddStatus(batchStatu);

            for (int i = 0; i < tokenBatch.PartsItemsCount; i++)
            {
                _tokenItemRepository.AddToken(new TokenItem
                {
                    GeneratedDate = DateTime.UtcNow,
                    IsGenerated = false,
                    IsProcessing = false,
                    IsUsed = false,
                    PartNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100000, 999999)),
                    PartPosition = i,
                    PinCode = KeyGenerator.GeneratePinCode(8).ToString(),
                    TokenBatch= tokenBatch
                });
            }

            return FirstOrDefault(b=>b.BatchNumber == tokenBatch.BatchNumber);
        }

        public  void Remove(TokenBatch tokenBatch)
        {
            _functionalDbContext.TokenBatches.Remove(tokenBatch.Id);
        }

        public Task<bool> Any(Expression<Func<TokenBatch, bool>> p) => Task.Run(() => _functionalDbContext.TokenBatches.Any(p));

        public Task<TokenBatch> FirstOrDefault(Expression<Func<TokenBatch, bool>> expression)
        {
            return Task.Run(() => _functionalDbContext.TokenBatches.FirstOrDefault(expression));
        }
    }
}
