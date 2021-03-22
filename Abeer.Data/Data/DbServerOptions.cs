namespace Abeer.Shared.Data
{
    public class DbServerOptions
    {
        public bool EnableDetailedErrors { get; set; }
        public bool EnableSensitiveDataLogging { get; set; }
        public string ConnectionString { get; set; }
        public int MaxBatchSize { get; set; }
        public string MigrationAssemblyName { get; set; }
        public string DbContextType { get; set; }
    }
}