using Abeer.Data.Repositories;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Abeer.Data.UnitOfworks
{
    public class BlockChainUnitOfWork
    {
        private BlockChainRepository _BlockChainRepository;

        public BlockChainUnitOfWork(IServiceProvider serviceProvider, IBlockChainDbContext applicationDbContext)
        {
            ApplicationDbContext = applicationDbContext;
            ServiceProvider = serviceProvider;
        }

        public void DetectChanges()
        {
            ApplicationDbContext.DetectChanges();
        }

        public BlockChainRepository BlockChainRepository
        {
            get
            {
                if (_BlockChainRepository == null)
                    _BlockChainRepository = ActivatorUtilities.CreateInstance<BlockChainRepository>(ServiceProvider);

                return _BlockChainRepository;
            }
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

        private IBlockChainDbContext ApplicationDbContext { get; }
        private IServiceProvider ServiceProvider { get; }

        public void EnsureCreated()
        {
            ApplicationDbContext.EnsureCreated();
        }
    }
}
