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
    public class XlsWriter<TITem> : IDisposable where TITem : class, new()
    {
        private readonly Stream _stream;
        private readonly XLWorkbook _workbook;
        private readonly IXLWorksheet _worksheet;
        private readonly TableWriter<TITem> _tableWriter;

        public XlsWriter(ExportOptionsViewModel options, IList<TITem> items, Stream stream)
        {
            Items = items;
            _stream = stream;
            _workbook = new XLWorkbook();
            _worksheet = _workbook.Worksheets.Add(typeof(TITem).Name);
            _tableWriter = new TableFactory().CreateXlsxWriter<TITem>(_worksheet, 1, 1);
        }

        public IList<TITem> Items { get; }

        public void Dispose()
        {
            _workbook.Dispose();
        }

        public void Write()
        {
            _tableWriter.Write(Items, typeof(TITem).GetProperties().Select(p => p.Name).ToArray());
            _workbook.SaveAs(_stream);
        }
    }
}
