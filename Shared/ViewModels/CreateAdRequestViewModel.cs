using Abeer.Shared.Functional;
using System;
using System.Collections.Generic;

namespace Abeer.Shared.ViewModels
{
    public class CreateAdRequestViewModel
    {
        public AdViewModel Ad { get; set; }
        public AdPrice Price { get; set; }
        public List<FileData> Files { get; set; }
    }
    public class FileData
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
        public DateTime LastModified { get; set; }
        public byte[] Data { get; set; }
    }
}
