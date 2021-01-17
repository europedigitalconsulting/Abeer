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
        public CardRepository(FunctionalDbContext applicationDbContext)
        {
            FunctionalDbContext = applicationDbContext;
        }
        public FunctionalDbContext FunctionalDbContext { get; }

        public Task<IList<Card>> GetCards() =>
            Task.Run(() => FunctionalDbContext.Cards.ToList());

        public Task<Card> GetCard(Guid id) =>
            Task.Run(() => FunctionalDbContext.Cards.FirstOrDefault(b => b.Id == id));


        public Task Update(Card card)
        {
            return Task.Run(() => FunctionalDbContext.Cards.Update(card));
        }

        private static readonly Random rdm = new Random();

        public async Task<IEnumerable<Card>> AddRange(Batch batch, IEnumerable<Card> cards, string userId)
        {
            return await Task.Run<IEnumerable<Card>>(() =>
            {
                if (cards.Any(c => c.BatchId == Guid.Empty))
                {
                    foreach (var card in cards.Where(c => c.BatchId == Guid.Empty))
                    {
                        card.Batch = batch;
                    }
                }

                FunctionalDbContext.Cards.AddRange(cards);
                return FunctionalDbContext.Cards.Where(b => b.BatchId == batch.Id).ToList();
            });
        }

        public async Task<Card> Add(Card card, string userId)
        {
            if (card.Batch == null && card.BatchId == Guid.Empty)
            {
                var batch = new Batch
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1,
                    CardType = card.CardType,
                    CardStartNumber = card.CardNumber,
                    CardLastNumber = card.CardNumber
                };

                batch = await AddBatch(batch, userId);
                card.BatchId = batch.Id;
            }

            card = FunctionalDbContext.Cards.Add(card);

            CardStatu cardStatu = new CardStatu
            {
                Card = card,
                StatusDate = DateTime.UtcNow,
                Status = CardStatus.Created,
                UserId = userId
            };

            await AddStatus(cardStatu);

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

            return card;
        }

        public Task<IList<Batch>> GetBatches() =>
            Task.Run(() => FunctionalDbContext.Batches.ToList());

        public async Task<Batch> AddBatch(Batch batch, string userId)
        {
            if (batch.Id == Guid.Empty)
                batch.Id = Guid.NewGuid();

            return await Task.Run(() => FunctionalDbContext.Batches.Add(batch));
        }

        public Task<string[]> GetCardTypes()
        {
            return Task.Run(() => (FunctionalDbContext.Cards.ToList())
                .Select(b => b.CardType).Distinct().OrderBy(s => s).ToArray());
        }

        public Task<Card> Find(Guid id)
        {
            return Task.Run(() => FunctionalDbContext.Cards.FirstOrDefault(c => c.Id == id));
        }

        public void Remove(Card card)
        {
            FunctionalDbContext.Cards.Remove(card.Id);
        }

        public Task<bool> Any(Expression<Func<Card, bool>> p)
        {

            return Task.Run(() => FunctionalDbContext.Cards.Any(p));
        }

        public Task<IList<Card>> Where(Expression<Func<Card, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.Cards.Where(expression));
        }

        public Task<CardStatu> AddStatus(CardStatu cardStatu)
        {
            return Task.Run(() => FunctionalDbContext.CardStatus.Add(cardStatu));
        }

        public Task<Card> FirstOrDefault(Expression<Func<Card, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.Cards.FirstOrDefault(expression));
        }

        public async Task UpdateBatch(Batch batch)
        {
            await Task.Run(() => FunctionalDbContext.Batches.Update(batch));
        }
    }
}
