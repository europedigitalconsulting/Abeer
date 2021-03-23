using System;

namespace Abeer.Shared.ViewModels
{
    public class ExportFileViewModel
    {
        public string FileMimeType { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public DateTime FileLastChanged { get; set; }
    }
}
