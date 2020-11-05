using Abeer.Shared.Data;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DbProvider.LiteDbProvider
{
    public class LiteDbProvider : IDbProvider
    {
        LiteDatabase liteDatabase;
        private int batchSize;
        private TimeSpan timeout;

        public LiteDbProvider(IConfiguration configuration)
        {
            var options = new LiteDbProviderOptions
            {
                FileName = configuration["Service:Database:FileName"],
                BatchSize = int.TryParse(configuration["Service:Database:BatchSize"], out var batchSize) ?
                    batchSize : 5000,
                Connection = configuration["Service.Database:Connection"] ?? "Shared"
            };

            if (!string.IsNullOrEmpty(configuration["Service:Database:Password"]))
                options.Password = configuration["Service:Database:Password"];

            var connectionString = new ConnectionString
            {
                Filename = options.FileName, 
                Upgrade = true
            };

            var connection = ConnectionType.Direct;

            if (!string.IsNullOrEmpty(options.Connection))
                Enum.TryParse<ConnectionType>(options.Connection, out connection);

            connectionString.Connection = connection;

            if (!string.IsNullOrEmpty(options.Password))
            {
                connectionString.Password = options.Password;
            }

            if (options.InitialSize > 0)
                connectionString.InitialSize = options.InitialSize;

            if (!string.IsNullOrEmpty(options.Collation))
                connectionString.Collation = new Collation(options.Collation);

            if (options.BatchSize > 0)
                batchSize = options.BatchSize;

            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            var fileName = Path.Combine(dataFolder, connectionString.Filename);

            connectionString.Filename = fileName;
            liteDatabase = new LiteDatabase(connectionString);

            timeout = TimeSpan.FromMinutes(5);
        }

        public void BulkInsert<T>(IList<T> entities) where T : class
        {
            var col = liteDatabase.GetCollection<T>(typeof(T).Name);
            col.InsertBulk(entities, batchSize);
        }

        public void BulkUpdate<T>(IList<T> entities) where T : class
        {
            var col = liteDatabase.GetCollection<T>(typeof(T).Name);
            col.Update(entities);
        }

        public void EnsureCreated()
        {
        }

        public int SaveChanges()
        {
            return -1;
        }

        public IDbSet<T> Set<T>() where T : class
        {
            return new LiteDbSet<T>(liteDatabase);
        }

        public void SetTimeout(int timeout)
        {
            this.timeout = TimeSpan.FromSeconds(timeout);
        }
    }
}
