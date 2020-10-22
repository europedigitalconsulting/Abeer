using Abeer.Data.Models;
using Abeer.Shared;
using Abeer.Shared.Functional;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Data
{
    public interface IFunctionalDbContext
    {
        DbSet<UrlShortned> UrlShortneds { get; }
        DbSet<CustomLink> CustomLinks { get; }
        DbSet<SocialNetwork> SocialNetworks { get; }
        DbSet<Contact> Contacts { get; }
        DbSet<TokenBatch> TokenBatches { get; }
        DbSet<Card> Cards { get; }
        DbSet<TokenItem> TokenItems { get; }
        DbSet<TokenBatchStatu> TokenBatchStatus { get; }
        DbSet<CardStatu> CardStatus { get; }
        DbSet<Wallet> Wallets { get; }
        DbSet<Shared.Transaction> Transactions { get; }
        DbSet<TransactionStatu> TransactionStatus { get; }
        DbSet<Shared.Purchase> Purchase { get; }
        DbSet<Shared.Payment> Payment { get; }
        DbSet<Country> Countries { get; }
        DbSet<AdModel> Ads { get; }
        DbSet<AdPrice> AdPrices { get; }
        Task BulkInsertAsync<T>(IEnumerable<T> countries) where T:class;
        Task BulkUpdateAsync<T>(IList<T> entities) where T : class;

        Task<int> SaveChangesAsync();
        int SaveChange();

        Task Update(object entity);

        void EnsureCreated();
        Task DetectChanges();
        void SetTimeout(int timeout);
    }
}