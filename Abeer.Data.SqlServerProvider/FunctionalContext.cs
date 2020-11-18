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
            optionsBuilder.UseSqlServer("Data Source=blog.db");
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

        public DbSet<UrlShortned> UrlShortned { get; set; }
        public DbSet<Contact> Contact { get; set; }
        public DbSet<TokenBatch> TokenBatch { get; set; }
        public DbSet<Card> Card { get; set; }
        public DbSet<TokenItem> TokenItem { get; set; }
        public DbSet<TokenBatchStatu> TokenBatchStatu { get; set; }
        public DbSet<CardStatu> CardStatu { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<SocialNetwork> SocialNetwork { get; set; }
        public DbSet<CustomLink> CustomLink { get; set; }
        public DbSet<AdModel> Ad { get; set; }
        public DbSet<AdPrice> AdPrice { get; set; }
    }
}
