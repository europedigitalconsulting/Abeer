using System;
using System.Collections.Generic;
using System.Text;
using AbeerContactShared.Data.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Areas.Identity.Data;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext() : base()
        {

        }

        public DbSet<IdentityUserClaim<string>> IdentityUserClaim { get; set; }
        public virtual DbSet<ApplicationUser> Utilisateurs { get; set; }
        public virtual DbSet<SocialLink> SocialLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserClaim<string>>().HasKey(p => new { p.Id });
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "Users");
                entity.Property(e => e.Id).HasColumnName("UserId");

            });
        }
    }
}
