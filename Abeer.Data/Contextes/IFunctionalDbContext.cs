using Abeer.Data.Models;
using Abeer.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Data
{
    public interface IFunctionalDbContext
    {
        DbSet<UrlShortned> UrlShortneds { get; set; }
        DbSet<Contact> Contacts { get; set; }
        DbSet<TokenBatch> TokenBatches { get; set; }
        DbSet<Card> Cards { get; set; }
        DbSet<TokenItem> TokenItems { get; set; }
        DbSet<TokenBatchStatu> TokenBatchStatus { get; set; }
        DbSet<CardStatu> CardStatus { get; set; }
        DbSet<Wallet> Wallets { get; set; }
        DbSet<Shared.Transaction> Transactions { get; set; }
        DbSet<TransactionStatu> TransactionStatus { get; set; }
        DbSet<Shared.Purchase> Purchase { get; set; }
        DbSet<Shared.Payment> Payment { get; set; }
        DbSet<Country> Countries { get; set; }

        Task BulkInsertAsync<T>(IEnumerable<T> countries) where T:class;
        Task BulkUpdateAsync<T>(IList<T> entities) where T : class;

        Task<int> SaveChangesAsync();
        int SaveChange();

        Task Update(object entity);

        void EnsureCreated();
        Task DetectChanges();
    }
}