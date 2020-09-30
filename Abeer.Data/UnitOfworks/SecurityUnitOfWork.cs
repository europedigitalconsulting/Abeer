
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Data.UnitOfworks
{
    public class SecurityUnitOfWork
    {
        public SecurityUnitOfWork(IServiceProvider serviceProvider, ISecurityDbContext applicationDbContext)
        {
            ApplicationDbContext = applicationDbContext;
            ServiceProvider = serviceProvider;
        }

        public void DetectChanges()
        {
            ApplicationDbContext.DetectChanges();
        }

        public int SaveChanges()
        {
            return ApplicationDbContext.SaveChange();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await ApplicationDbContext.SaveChangesAsync();
        }

        public Task BulkUpdateAsync<T>(IList<T> entities) where T : class
        {
            return ApplicationDbContext.BulkUpdateAsync<T>(entities);
        }

        private ISecurityDbContext ApplicationDbContext { get; }
        private IServiceProvider ServiceProvider { get; }

        public void EnsureCreated()
        {
            ApplicationDbContext.EnsureCreated();
        }
    }
}
