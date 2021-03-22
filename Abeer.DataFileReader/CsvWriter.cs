using Abeer.Shared.ViewModels;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableIO;
using TableIO.ClosedXml;

namespace Abeer.DataFileReader
{
    public class CsvWriter<TITem> : IDisposable where TITem : class, new()
    {
        private readonly TableWriter<TITem> _tableWriter;

        public CsvWriter(ExportOptionsViewModel options, IList<TITem> items, StreamWriter stream)
        {
            Items = items;
            _tableWriter = new TableFactory().CreateCsvWriter<TITem>(stream);
        }

        public IList<TITem> Items { get; }

        public void Dispose()
        {
        }

        public void Write()
        {
            _tableWriter.Write(Items, typeof(TITem).GetProperties().Select(p => p.Name).ToArray());
        }
    }
}
