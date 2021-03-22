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
        private readonly DbServerOptions dbServerOptions;
        private readonly DbContext _sqlContext;

        public SqlServerDbProvider(IServiceProvider sp, IConfiguration configuration) :
    this(sp, configuration, "Service:Database:Core:ConnectionStrings",
        "Service:Database:Core:EnableDetailedErrors",
        "Service:Database:Core:EnableSensitiveDataLogging",
        "Service:Database:Core:MigrationAssemblyName",
        "Service:Database:Core:DbContext")
        {
        }
        public SqlServerDbProvider(IServiceProvider sp, DbServerOptions dbServerOptions)
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.EnableDetailedErrors(dbServerOptions.EnableDetailedErrors);
            dbContextOptionsBuilder.EnableSensitiveDataLogging(dbServerOptions.EnableSensitiveDataLogging);
            dbContextOptionsBuilder.UseSqlServer(
                dbServerOptions.ConnectionString, c =>
                {
                    c.EnableRetryOnFailure();

                    if (dbServerOptions.MaxBatchSize > 0)
                        c.MaxBatchSize(dbServerOptions.MaxBatchSize);

                    c.MigrationsAssembly(dbServerOptions.MigrationAssemblyName);
                });

            _sqlContext = (DbContext)ActivatorUtilities
                .CreateInstance(sp,
                    Type.GetType(dbServerOptions.DbContextType) ?? throw new InvalidOperationException(), dbContextOptionsBuilder.Options);

            //_sqlContext.Database.EnsureCreated();
            _sqlContext.Database.Migrate();
        }
        public SqlServerDbProvider(IServiceProvider sp, IConfiguration configuration,
             string connectionStringKey, string enableDetailedErrorsKey, string enableSensitiveDataLoggingKey,
             string migrationAssemblyKey, string dbContextKey) :
             this(sp, new DbServerOptions
             {
                 ConnectionString = configuration.GetConnectionString(configuration[connectionStringKey]),
                 EnableDetailedErrors = bool.TryParse(configuration[enableDetailedErrorsKey], out var enableDetailedErrors) && enableDetailedErrors,
                 EnableSensitiveDataLogging = bool.TryParse(configuration[enableSensitiveDataLoggingKey], out var enableSensitiveDataLogging) && enableSensitiveDataLogging,
                 MigrationAssemblyName = configuration[migrationAssemblyKey],
                 DbContextType = configuration[dbContextKey]
             })
        {
        }
        public void BulkInsert<T>(IList<T> entities) where T : class
        {
            _sqlContext.BulkInsert<T>(entities);
        }

        public void BulkDelete<T>(IList<T> entities) where T : class
        {
            _sqlContext.BulkDelete(entities);
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