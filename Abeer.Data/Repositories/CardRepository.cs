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
    public class CardRepository
    {
        public CardRepository(IFunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }

        public async Task<List<Card>> GetCards() =>
            await FunctionalDbContext.Cards.Include(c => c.CardStatus).ToListAsync();

        public async Task<Card> GetCard(Guid id) =>
            await FunctionalDbContext.Cards.Include(c => c.CardStatus).FirstOrDefaultAsync(b => b.Id == id);

        public IFunctionalDbContext FunctionalDbContext { get; }

        public async Task Update(Card card)
        {
            FunctionalDbContext.Cards.Update(card);
            await FunctionalDbContext.SaveChangesAsync();
        }

        static readonly Random rdm = new Random();

        public async Task<Card> Add(Card card, string userId)
        {
            var entity = await FunctionalDbContext.Cards.AddAsync(card);
            await FunctionalDbContext.SaveChangesAsync();

            card = entity.Entity;

            CardStatu cardStatu = new CardStatu { Card = card, 
                StatusDate = DateTime.UtcNow, 
                Status = CardStatus.Created, 
                UserId = userId
            };

            await AddStatus(cardStatu);

            var tokens = await FunctionalDbContext.TokenItems.Include(t => t.TokenBatch)
                .Where(t => t.TokenBatch.TokenType == card.CardType && t.IsUsed == false && t.IsError == false)
                .ToListAsync();

            var batchRepo = new TokenBatchRepository(FunctionalDbContext);
            var tokenItemRepo = new TokenItemRepository(FunctionalDbContext);

            var available = tokens.Count;

            if(available >= card.Value)
            {
                tokens = tokens.Take(card.Value).ToList();
            }
            else
            {
                var requiredTokens = card.Value - available;

                var batch = new TokenBatch
                {
                    BatchNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100000, 999999)),
                    OperatorName = "sys",
                    PartsItemsCount = requiredTokens,
                    TokenType = card.CardType
                };

                if(tokens.Any())
                {
                    var firstBatch = tokens.First().TokenBatch;
                    batch.WebHookUrl = firstBatch.WebHookUrl;
                    batch.WebHookProtocolType = firstBatch.WebHookProtocolType;
                    batch.WebHookAuthentication = firstBatch.WebHookAuthentication;
                }

                batch = (await batchRepo.AddAsync(batch, userId));
                tokens.AddRange(batch.TokenItems);
            }

            foreach(var token in tokens)
            {
                token.IsUsed = true;
                token.UsedDate = DateTime.UtcNow;
                token.CardNumber = card.CardNumber;
                token.CardId = card.Id;
                await tokenItemRepo.Update(token);
            }

            await AddStatus(new CardStatu
            {
                Card = card,
                Status = CardStatus.Generated,
                StatusDate = DateTime.UtcNow,
                UserId = userId
            });

            card.IsGenerated = true;
            card.GeneratedDate = DateTime.UtcNow;
            card.GeneratedBy = userId;

            await Update(card);

            return await FindAsync(card.Id);
        }

        public async Task<string[]> GetCardTypes()
        {
            return await FunctionalDbContext.TokenBatches
                .Select(b => b.TokenType).Distinct().OrderBy(s => s).ToArrayAsync();
        }

        public async Task<Card> FindAsync(Guid id)
        {
            return await FunctionalDbContext.Cards.FindAsync(id);
        }

        public void Remove(Card card)
        {
            FunctionalDbContext.Cards.Remove(card);
        }

        IIncludableQueryable<Card, List<CardStatu>> IncludableQueryables => FunctionalDbContext.Cards.Include(c => c.CardStatus);

        public Task<bool> AnyAsync(Expression<Func<Card, bool>> p)
        {
            
            return IncludableQueryables.AnyAsync(p);
        }

        public Task<List<Card>> Where(Expression<Func<Card, bool>> expression)
        {
            return IncludableQueryables.Where(expression).ToListAsync();
        }

        public async ValueTask<CardStatu> AddStatus(CardStatu cardStatu)
        {
            var entity = await FunctionalDbContext.CardStatus.AddAsync(cardStatu);
            await FunctionalDbContext.SaveChangesAsync();
            return entity.Entity;
        }

        public Task<Card> FirstOrDefaultAsync(Expression<Func<Card, bool>> expression)
        {
            return IncludableQueryables.FirstOrDefaultAsync(expression);
        }
    }
}
