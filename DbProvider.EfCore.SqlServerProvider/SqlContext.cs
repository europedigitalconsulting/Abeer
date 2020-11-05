using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbProvider.EfCore.SqlServerProvider
{
    public class SqlContext: IDisposable 
    {
        protected internal readonly DbContext _dbContext;

        public SqlContext(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(
                        nameof(dbContext), 
                        $"The parameter dbContext can not be null");
        }

        public DbSet<TEntity> Set<TEntity>()
            where TEntity:class, new()
            => _dbContext.Set<TEntity>();

        public void Dispose()
        {
            if (_dbContext != null) _dbContext.Dispose();
        }
    }
}
