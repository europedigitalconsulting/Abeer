using Abeer.Shared.Data;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DbProvider.EfCore.SqlServerProvider
{
    public class SqlServerDbProvider : IDbProvider
    {
        private readonly SqlServerOptions sqlServerOptions;
        private readonly DbContext _sqlContext;

        public SqlServerDbProvider(IServiceProvider sp, IConfiguration configuration)
        {
            this.sqlServerOptions = new SqlServerOptions
            {
                ConnectionString = configuration.GetConnectionString(configuration["Service:Database:ConnectionStrings"]),
                EnableDetailedErrors = bool.TryParse(configuration["Service:Database:EnableDetailedErrors"], out var enableDetailedErrors) ?
                    enableDetailedErrors : false, 
                EnableSensitiveDataLogging = bool.TryParse(configuration["Service:Database:EnableSensitiveDataLogging"], out var enableSensitiveDataLogging) ?
                    enableSensitiveDataLogging : false,
                MigrationAssemblyName = configuration["Service:Database:MigrationAssemblyName"],
                DbContextType = configuration["Service:Database:DbContext"]
            };

            var dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.EnableDetailedErrors(sqlServerOptions.EnableDetailedErrors);
            dbContextOptionsBuilder.EnableSensitiveDataLogging(sqlServerOptions.EnableSensitiveDataLogging);
            dbContextOptionsBuilder.UseSqlServer(
                sqlServerOptions.ConnectionString, c=>
            {
                c.EnableRetryOnFailure();

                if(sqlServerOptions.MaxBatchSize > 0)
                    c.MaxBatchSize(sqlServerOptions.MaxBatchSize);

                c.MigrationsAssembly(sqlServerOptions.MigrationAssemblyName);
            });

            _sqlContext = (DbContext)ActivatorUtilities
                .CreateInstance(sp, 
                    Type.GetType(sqlServerOptions.DbContextType), dbContextOptionsBuilder.Options);

            //_sqlContext.Database.EnsureCreated();
            _sqlContext.Database.Migrate();
        }

        public void BulkInsert<T>(IList<T> entities) where T : class
        {
            _sqlContext.BulkInsert<T>(entities);
        }

        public void BulkUpdate<T>(IList<T> entities) where T : class
        {
            _sqlContext.BulkUpdate<T>(entities);
        }

        public void EnsureCreated()
        {
            _sqlContext.Database.EnsureCreated();
        }

        public int SaveChanges()
        {
            return _sqlContext.SaveChanges();
        }

        public IDbSet<T> Set<T>() where T : class
        {
            return new SqlServerSet<T>(_sqlContext.Set<T>(), _sqlContext);
        }

        public void SetTimeout(int timeout)
        {
            _sqlContext.Database.SetCommandTimeout(timeout);
        }
    }
}