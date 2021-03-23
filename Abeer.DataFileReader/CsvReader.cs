using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abeer.Shared.ViewModels;

namespace Abeer.DataFileReader
{
    public class CsvReader<TItem> where TItem : class, new()
    {
        private readonly ImportOptionsViewModel _importOptions;
        private readonly Stream _stream;

        public CsvReader(ImportOptionsViewModel importOptions)
        {

            _importOptions = importOptions;
            _stream = new MemoryStream(importOptions.Data) {Position = 0};
        }


        public IList<TItem> Read()
        {
            using var reader = new StreamReader(_stream);
            var csvText = reader.ReadToEnd();
            var lines = csvText.Split(Environment.NewLine);
            var columns = GetColumns(lines);
            var firstRowIndex = 0;
            
            if (_importOptions.HasHeader)
                firstRowIndex++;

            if (_importOptions.SkipFirstRows > 0)
                firstRowIndex += _importOptions.SkipFirstRows;

            var items = new List<TItem>();

            for(var r = firstRowIndex; r < lines.Length; r++)
            {
                var item = new TItem();

                for(var c = 0;  c < columns.Count; c++)
                {
                    var key = columns.Keys.ToArray()[c];
                    var parts = lines[r].Split(';');

                    var property = typeof(TItem).GetProperty(key);
                    
                    var index = columns.Values.ToArray()[c];

                    if (parts.Length <= index)
                        break;

                    var s = parts[index];

                    if (string.IsNullOrEmpty(s))
                        continue;

                    if (property == null)
                        continue;

                    if (property.PropertyType.Name.Contains("Guid"))
                        property.SetValue(item, Guid.Parse(s));
                    else
                    {
                        var value = Convert.ChangeType(s, property.PropertyType);
                        property.SetValue(item, value);
                    }
                }

                items.Add(item);
            }

            return items;
        }

        private Dictionary<string, int> GetColumns(string[] lines)
        {
            var properties = typeof(TItem).GetProperties().Select(p => p.Name).ToList();
            var columns = new Dictionary<string, int>();

            var cols = _importOptions.HasHeader ? lines[0].Split(';') : properties.ToArray();

            for (var i = 0; i < cols.Length; i++)
            {
                if (properties.Contains(cols[i], StringComparer.OrdinalIgnoreCase))
                    columns.Add(cols[i], i);
            }

            return columns;
        }
    }
}
