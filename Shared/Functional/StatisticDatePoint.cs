using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Functional
{
    public class StatisticDatePoint
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
    }

    public class StatisticKeyPoint
    {
        public string Key { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }
}
