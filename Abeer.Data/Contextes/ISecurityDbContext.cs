using Abeer.Data.Models;
using Abeer.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Data
{
    public interface ISecurityDbContext
    {
        Task BulkInsertAsync<T>(IEnumerable<T> entities) where T:class;
        Task<int> SaveChangesAsync();
        int SaveChange();
        Task BulkUpdateAsync<T>(IList<T> entities) where T : class;
        Task Update(object entity);
        void EnsureCreated();
        Task DetectChanges();
    }
}