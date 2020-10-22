using Abeer.Data;

using LiteDB;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbProvider.LiteDbProvider
{
    public class LiteDbProvider : Abeer.Data.IDbProvider
    {
        LiteDatabase liteDatabase;
        private int batchSize;
        private TimeSpan timeout;

        public LiteDbProvider(IOptions<LiteDbProviderOptions> configuration)
        {
            var settings = configuration.Value;

            var connectionString = new ConnectionString
            {
                Filename = settings.FileName, 
                Upgrade = true
            };

            var connection = ConnectionType.Direct;

            if (!string.IsNullOrEmpty(settings.Connection))
                Enum.TryParse<ConnectionType>(settings.Connection, out connection);

            connectionString.Connection = connection;

            if (!string.IsNullOrEmpty(settings.Password))
            {
                connectionString.Password = settings.Password;
            }

            if (settings.InitialSize > 0)
                connectionString.InitialSize = settings.InitialSize;

            if (!string.IsNullOrEmpty(settings.Collation))
                connectionString.Collation = new Collation(settings.Collation);

            if (settings.BatchSize > 0)
                batchSize = settings.BatchSize;

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
