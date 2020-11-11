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

        public  Task<IList<Card>> GetCards() =>
            Task.Run(() => FunctionalDbContext.Cards.ToList());

        public  Task<Card> GetCard(Guid id) =>
            Task.Run(() => FunctionalDbContext.Cards.FirstOrDefault(b => b.Id == id));


        public  Task Update(Card card)
        {
            return Task.Run(() => FunctionalDbContext.Cards.Update(card));
        }

        static readonly Random rdm = new Random();

        public  Task<Card> Add(Card card, string userId)
        {
            return Task.Run(() =>
            {
                card = FunctionalDbContext.Cards.Add(card);

                CardStatu cardStatu = new CardStatu
                {
                    Card = card,
                    StatusDate = DateTime.UtcNow,
                    Status = CardStatus.Created,
                    UserId = userId
                };

                AddStatus(cardStatu);

                AddStatus(new CardStatu
                {
                    Card = card,
                    Status = CardStatus.Generated,
                    StatusDate = DateTime.UtcNow,
                    UserId = userId
                });

                card.IsGenerated = true;
                card.GeneratedDate = DateTime.UtcNow;
                card.GeneratedBy = userId;

                Update(card);

                return Find(card.Id);
            });
        }

        public  Task<string[]> GetCardTypes()
        {
            return Task.Run(() => (FunctionalDbContext.TokenBatches.ToList())
                .Select(b => b.TokenType).Distinct().OrderBy(s => s).ToArray());
        }

        public  Task<Card> Find(Guid id)
        {
            return Task.Run(() => FunctionalDbContext.Cards.FirstOrDefault(c=>c.Id == id));
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

        public  Task<CardStatu> AddStatus(CardStatu cardStatu)
        {
            return Task.Run(() => FunctionalDbContext.CardStatus.Add(cardStatu));
        }

        public Task<Card> FirstOrDefault(Expression<Func<Card, bool>> expression)
        {
            return Task.Run(() => FunctionalDbContext.Cards.FirstOrDefault(expression));
        }
    }
}
