namespace Abeer.Shared.ViewModels
{
    public class ExportOptionsViewModel
    {
        public bool HasHeader { get; set; }
        public string FileType { get; set; }
        public int LimitRows { get; set; }
        public bool IncludeFamily { get; set; }
        public bool IncludeCategory { get; set; }
        public bool IncludeProductType { get; set; }
        public string Family { get; set; }
        public string Category { get; set; }
        public string ProductType { get; set; }
    }
}
