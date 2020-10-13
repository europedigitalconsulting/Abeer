using Abeer.Data;
using Abeer.Data.Models;
using Abeer.Shared;
using Abeer.Shared.Functional;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Data.Contextes
{
    public class FunctionalDbContext : DbContext, IFunctionalDbContext
    {
        public FunctionalDbContext(DbContextOptions<FunctionalDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Primary keys
            builder.Entity<Country>().HasKey(p => p.Id);
            builder.Entity<TokenBatch>().HasKey(p => p.Id);
            builder.Entity<TokenBatchStatu>().HasOne(p => p.TokenBatch).WithMany(b => b.TokenBatchStatus);
            builder.Entity<TokenItem>().HasOne(p => p.TokenBatch).WithMany(b => b.TokenItems);
            builder.Entity<CardStatu>().HasOne(p => p.Card).WithMany(b => b.CardStatus);
            builder.Entity<PurchaseItem>().HasOne(p => p.Purchase).WithMany(t => t.PurchaseItems);
            builder.Entity<Purchase>().HasMany(p => p.Payments);
            builder.Entity<Purchase>().HasMany(p => p.TransactionStatus);
            builder.Entity<Payment>().HasOne(p => p.Transaction).WithMany(t => t.Payments);
            builder.Entity<TransactionStatu>().HasOne(p => p.Transaction).WithMany(t => t.TransactionStatus);
        }

        public int SaveChange()
        {
            return SaveChangesAsync().Result;
        }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        public Task Update(object entity)
        {
            Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task BulkUpdateAsync<T>(IList<T> entities) where T : class
        {
            Parallel.ForEach(entities, entity => Update(entity));
            return Task.CompletedTask;
        }

        public Task BulkInsertAsync<T>(IEnumerable<T> items) where T : class
        {
            foreach (var item in items)
                Add(item);

            return Task.CompletedTask;
        }

        public void EnsureCreated()
        {
            Database.EnsureCreated();
        }

        public Task DetectChanges()
        {
            ChangeTracker.DetectChanges();
            return Task.CompletedTask;
        }

        public DbSet<UrlShortned> UrlShortneds { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<TokenBatch> TokenBatches { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<TokenItem> TokenItems { get; set; }
        public DbSet<TokenBatchStatu> TokenBatchStatus { get; set; }
        public DbSet<CardStatu> CardStatus { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionStatu> TransactionStatus { get; set; }
        public DbSet<Purchase> Purchase { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<SocialNetwork> SocialNetworks { get; set; }
        public DbSet<CustomLink> CustomLinks { get; set; }
        public DbSet<OfferModel> Offers { get; set; }
    }
}