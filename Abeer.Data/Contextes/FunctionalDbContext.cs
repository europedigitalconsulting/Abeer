using Abeer.Data.Models;
using Abeer.Shared;
using Abeer.Shared.Data;
using Abeer.Shared.Functional;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Data
{
    public class FunctionalDbContext
    {
        private readonly IDbProvider dbProvider;

        public FunctionalDbContext(IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
        }

        public int SaveChange()
        {
            return dbProvider.SaveChanges();
        }

        public void BulkUpdate<T>(IList<T> entities) where T : class
        {
            dbProvider.BulkUpdate<T>(entities);
        }

        public void BulkInsert<T>(IList<T> entities) where T : class
        {
            dbProvider.BulkInsert<T>(entities);
        }

        public void EnsureCreated()
        {
            dbProvider.EnsureCreated();
        }

        public void SetTimeout(int timeout)
        {
            dbProvider.SetTimeout(timeout);
        }

        public IDbSet<UrlShortned> UrlShortneds => dbProvider.Set<UrlShortned>();
        public IDbSet<Contact> Contacts => dbProvider.Set<Contact>();
        public IDbSet<TokenBatch> TokenBatches => dbProvider.Set<TokenBatch>();
        public IDbSet<Card> Cards => dbProvider.Set<Card>();
        public IDbSet<TokenItem> TokenItems => dbProvider.Set<TokenItem>();
        public IDbSet<TokenBatchStatu> TokenBatchStatus => dbProvider.Set<TokenBatchStatu>();
        public IDbSet<CardStatu> CardStatus => dbProvider.Set<CardStatu>();
        public IDbSet<Wallet> Wallets => dbProvider.Set<Wallet>();
        public IDbSet<Transaction> Transactions => dbProvider.Set<Transaction>();
        public IDbSet<TransactionStatu> TransactionStatus => dbProvider.Set<TransactionStatu>();
        public IDbSet<Purchase> Purchase => dbProvider.Set<Purchase>();
        public IDbSet<Payment> Payments => dbProvider.Set<Payment>();
        public IDbSet<Country> Countries => dbProvider.Set<Country>();
        public IDbSet<SocialNetwork> SocialNetworks => dbProvider.Set<SocialNetwork>();
        public IDbSet<CustomLink> CustomLinks => dbProvider.Set<CustomLink>();
        public IDbSet<AdModel> Ads => dbProvider.Set<AdModel>();
        public IDbSet<AdPrice> AdPrices => dbProvider.Set<AdPrice>();
    }
}