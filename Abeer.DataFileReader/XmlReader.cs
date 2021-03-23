using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Abeer.Shared.ViewModels;

namespace Abeer.DataFileReader
{
    public class XmlReader<TItem> where TItem : class, new()
    {
        private readonly XmlDocument _xDoc;

        public XmlReader(ImportOptionsViewModel importOptions)
        {
            var stream = new MemoryStream(importOptions.Data)
            {
                Position = 0
            };

            using var reader = new StreamReader(stream);

            var xml = reader.ReadToEnd();
            
            _xDoc = new XmlDocument();
            _xDoc.LoadXml(xml);
        }


        public IList<TItem> Read()
        {
            return (from XmlNode node in _xDoc?.DocumentElement?.ChildNodes select ParseNode(node)).ToList();
        }

        public TItem ParseNode(XmlNode node)
        {
            var item = new TItem();

            foreach (var property in item.GetType().GetProperties())
            {
                if (!property.CanWrite)
                    continue;

                XmlNode element = node[property.Name];

                if (element != null)
                {
                    var text = element.InnerText;
                    var value = Convert.ChangeType(text, property.PropertyType);
                    property.SetValue(item, value);
                }
            }

            return item;
        }

        public XmlNode SelectNode(string xPath)
        {
            return _xDoc.SelectSingleNode(xPath);
        }
    }
}
