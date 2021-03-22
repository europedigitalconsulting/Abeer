using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Ads.Shared
{
    public class AdsImportResultViewModel
    {
        public int FamiliesAdded { get; set; }
        public int FamiliesUpdated { get; set; }
        public int CategoriesAdded { get; set; }
        public int CategoriesUpdated { get; set; }
        public int ProductTypesAdded { get; set; }
        public int ProductTypesUpdated { get; set; }
    }

    public class ExportResultViewModel
    {
        public int FamiliesExported { get; set; }
        public int CategoriesExported { get; set; }
        public int ProductTypesExported { get; set; }
        public string FileName { get; set; }
        public string FileMimeType { get; set; }
        public byte[] FileContent { get; set; }
    }
}
