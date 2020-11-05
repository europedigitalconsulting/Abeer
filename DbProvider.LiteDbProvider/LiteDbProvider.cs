using Abeer.Data;

using LiteDB;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DbProvider.LiteDbProvider
{
    public class LiteDbProvider : Abeer.Data.IDbProvider
    {
        LiteDatabase liteDatabase;
        private int batchSize;
        private TimeSpan timeout;

        public LiteDbProvider(LiteDbProviderOptions configuration)
        {
            var connectionString = new ConnectionString
            {
                Filename = configuration.FileName, 
                Upgrade = true
            };

            var connection = ConnectionType.Direct;

            if (!string.IsNullOrEmpty(configuration.Connection))
                Enum.TryParse<ConnectionType>(configuration.Connection, out connection);

            connectionString.Connection = connection;

            if (!string.IsNullOrEmpty(configuration.Password))
            {
                connectionString.Password = configuration.Password;
            }

            if (configuration.InitialSize > 0)
                connectionString.InitialSize = configuration.InitialSize;

            if (!string.IsNullOrEmpty(configuration.Collation))
                connectionString.Collation = new Collation(configuration.Collation);

            if (configuration.BatchSize > 0)
                batchSize = configuration.BatchSize;

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
