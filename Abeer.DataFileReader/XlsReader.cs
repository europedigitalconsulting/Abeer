using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TableIO;
using TableIO.ClosedXml;
using Abeer.Shared.ViewModels;

namespace Abeer.Services
{
    public class XlsReader<TItem> : IDisposable where TItem : class, new()
    {
        private readonly Stream _stream;
        private readonly XLWorkbook _workbook;
        private readonly IXLWorksheet _worksheet;
        private readonly TableReader<TItem> _tableReader;

        public XlsReader(ImportOptionsViewModel importOptions)
        {
            _stream = new MemoryStream(importOptions.Data);
            _workbook = new XLWorkbook(_stream);
            _worksheet = _workbook.Worksheet(1);
            _tableReader = new TableFactory().CreateXlsxReader<TItem>(_worksheet, 
                importOptions.SkipFirstRows > 0 ? importOptions.SkipFirstRows : 1, 1, 
                typeof(TItem).GetProperties().Length, importOptions.HasHeader);
        }

        public IList<TItem> Read() => _tableReader.Read().ToList();

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _worksheet.Dispose();
                _workbook.Dispose();
                _stream.Close();
                _stream.Dispose();
            }
        }
    }
}