namespace DbProvider.LiteDbProvider
{
    public class LiteDbProviderOptions
    {
        public string FileName { get; set; }
        public long InitialSize { get; set; }
        public string Connection { get; set; }
        public string Password { get; set; }
        public string Collation { get; set; }
        public int BatchSize { get; set; }
    }
}
