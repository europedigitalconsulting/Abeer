using Abeer.Data.Models;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abeer.Data.SqlServerProvider
{
    public class FunctionalContextFactory : IDesignTimeDbContextFactory<FunctionalContext>
    {
        public FunctionalContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FunctionalContext>();
            optionsBuilder.UseSqlServer("Server=tcp:sqledc.database.windows.net,1433;Initial Catalog=dev-abeer;Persist Security Info=False;User ID=adminsql;Password=Xc9wf8or2020&;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            return new FunctionalContext(optionsBuilder.Options);
        }
    }

    public class FunctionalContext : DbContext
    {
        public FunctionalContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Batch> Batches { get; set; }
        public DbSet<UrlShortned> UrlShortned { get; set; }
        public DbSet<Contact> Contact { get; set; } 
        public DbSet<CardStatu> CardStatu { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<SocialNetwork> SocialNetwork { get; set; }
        public DbSet<CustomLink> CustomLink { get; set; }
        public DbSet<AdModel> Ad { get; set; }
        public DbSet<AdPrice> AdPrice { get; set; }
        public DbSet<PaymentModel> Payment { get; set; }
        public DbSet<SubscriptionPack> SubscriptionPack { get; set; }
        public DbSet<SubscriptionHistory> SubscriptionHistory { get; set; }
    }
}
