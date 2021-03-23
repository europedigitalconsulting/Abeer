using Abeer.Shared.ViewModels;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TableIO;
using TableIO.ClosedXml;

namespace Abeer.DataFileReader
{
    public class XmlWriter<TITem> : IDisposable where TITem : class, new()
    {
        private XmlSerializer _xmlSerializer;
        private readonly StreamWriter _stream;

        public XmlWriter(ExportOptionsViewModel options, IList<TITem> items, StreamWriter stream)
        {
            Items = items;
            
            _stream = stream;
            _xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(TITem));
        }

        public IList<TITem> Items { get; }

        public void Dispose()
        {
        }

        public void Write()
        {
            foreach (var item in Items)
                _xmlSerializer.Serialize(_stream, item);
        }
    }
}
