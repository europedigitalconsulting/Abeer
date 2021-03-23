namespace Abeer.Shared.ViewModels
{
    public class ImportOptionsViewModel
    {
        public int SkipFirstRows { get; set; }
        public int LimitRows { get; set; }
        public byte[] Data { get; set; }
        public bool HasHeader { get; set; }
        public bool Reset { get; set; }
        public string FileName { get; set; }
        public string TypeImport { get; set; }
    }
}
