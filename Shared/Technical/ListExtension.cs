using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Technical
{
    public static class ListExtension
    {
        public static int FindIndex<TItem>(this IList<TItem> items, Func<TItem, bool> filter)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (filter(items[i]))
                    return i;
            }

            return -1;
        }
    }
}
