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
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Meetag;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EventTrackingItem> EventTrackingItems { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
    }
}
