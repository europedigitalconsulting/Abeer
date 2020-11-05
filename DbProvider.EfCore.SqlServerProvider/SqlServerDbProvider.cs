using Abeer.Data;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DbProvider.EfCore.SqlServerProvider
{
    public class SqlServerDbProvider : Abeer.Data.IDbProvider
    {
        private readonly SqlServerOptions sqlServerOptions;
        private readonly SqlContext _sqlContext;

        public SqlServerDbProvider(SqlServerOptions sqlServerOptions)
        {
            this.sqlServerOptions = sqlServerOptions;
            var dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.EnableDetailedErrors(sqlServerOptions.EnableDetailedErrors);
            dbContextOptionsBuilder.EnableSensitiveDataLogging(sqlServerOptions.EnableSensitiveDataLogging);
            dbContextOptionsBuilder.UseSqlServer(
                sqlServerOptions.ConnectionString, c=>
            {
                c.EnableRetryOnFailure();
                c.MaxBatchSize(sqlServerOptions.MaxBatchSize);
            });

            _sqlContext = new SqlContext(
                new DbContext(dbContextOptionsBuilder.Options)
                );
        }

        public void BulkInsert<T>(IList<T> entities) where T : class
        {
            _sqlContext._dbContext.BulkInsert<T>(entities);
        }

        public void BulkUpdate<T>(IList<T> entities) where T : class
        {
            _sqlContext._dbContext.BulkUpdate<T>(entities);
        }

        public void EnsureCreated()
        {
            _sqlContext._dbContext.Database.EnsureCreated();
        }

        public int SaveChanges()
        {
            return _sqlContext._dbContext.SaveChanges();
        }

        public IDbSet<T> Set<T>() where T : class
        {
            return new SqlServerSet<T>(_sqlContext._dbContext.Set<T>());
        }

        public void SetTimeout(int timeout)
        {
            _sqlContext._dbContext.Database.SetCommandTimeout(timeout);
        }
    }
}