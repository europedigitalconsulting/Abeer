using Abeer.Ads.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Abeer.Ads.Data.SqlServerProvider
{ 
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AdsContext>
    {
        public AdsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AdsContext>();
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=meetag;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            return new AdsContext(optionsBuilder.Options);
        }
    }

    public class AdsContext : DbContext
    {
        public AdsContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AdsFamily>().HasKey(c => c.FamilyId);

        }

        public DbSet<AdsCategory> AdsCategories { get; set; }
        public DbSet<AdsFamily> AdsFamilies { get; set; }
        public DbSet<AdsFamilyAttribute> AdsFamilyAttributes { get; set; }
        public DbSet<CategoryAd> CategoryAds { get; set; }
    }
}
