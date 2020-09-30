
using Abeer.Data;
using Abeer.Data.Models;
using Abeer.Shared;

using IdentityServer4.EntityFramework.Options;

using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Server.Data
{
    public class SecurityDbContext : ApiAuthorizationDbContext<ApplicationUser>, ISecurityDbContext
    {
        public SecurityDbContext(
            DbContextOptions<SecurityDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public int SaveChange()
        {
            return SaveChangesAsync().Result;
        }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        Task ISecurityDbContext.Update(object entity)
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
    }
}
