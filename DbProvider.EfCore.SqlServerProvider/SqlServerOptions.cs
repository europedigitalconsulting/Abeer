﻿namespace DbProvider.EfCore.SqlServerProvider
{
    public class SqlServerOptions
    {
        public bool EnableDetailedErrors { get; set; }
        public bool EnableSensitiveDataLogging { get; set; }
        public string ConnectionString { get; set; }
        public int MaxBatchSize { get; set; }
    }
}