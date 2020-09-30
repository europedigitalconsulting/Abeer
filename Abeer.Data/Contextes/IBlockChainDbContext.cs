using Abeer.Data.Models;
using Abeer.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Data
{
    public interface IBlockChainDbContext
    {
        DbSet<TokenBlockChain> TokenBlockChains { get; set; }
        DbSet<WalletBlockChain> WalletBlockChains { get; set; }
        DbSet<BatchBlockChain> BatchBlockChains { get; set; }
        DbSet<CardBlockChain> CardBlockChains { get; set; }
        DbSet<Block> Blocks { get; set; }

        Task BulkInsertAsync<T>(IEnumerable<T> countries) where T:class;
        Task BulkUpdateAsync<T>(IList<T> entities) where T : class;

        int SaveChange();

        Task<int> SaveChangesAsync();
        Task Update(object entity);

        void EnsureCreated();
        Task DetectChanges();
    }
}