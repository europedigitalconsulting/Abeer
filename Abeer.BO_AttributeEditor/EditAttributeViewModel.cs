using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.AttributeEditor
{
    public class EditAttributeViewModel
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSearchable { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string RequiredMsg { get; set; }
    }
}
