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
        public IDbSet<Card> Cards => dbProvider.Set<Card>();
        public IDbSet<CardStatu> CardStatus => dbProvider.Set<CardStatu>();
        public IDbSet<Country> Countries => dbProvider.Set<Country>();
        public IDbSet<SocialNetwork> SocialNetworks => dbProvider.Set<SocialNetwork>();
        public IDbSet<CustomLink> CustomLinks => dbProvider.Set<CustomLink>();
        public IDbSet<AdModel> Ads => dbProvider.Set<AdModel>();
        public IDbSet<AdPrice> AdPrices => dbProvider.Set<AdPrice>();
        public IDbSet<PaymentModel> Payments => dbProvider.Set<PaymentModel>();
        public IDbSet<SubscriptionPack> SubscriptionPacks => dbProvider.Set<SubscriptionPack>();
    }
}