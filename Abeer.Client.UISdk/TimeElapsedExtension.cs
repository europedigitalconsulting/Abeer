using System;
using System.Collections.Generic;
using System.Linq;

namespace Abeer.Client
{
    public static class TimeElapsedExtension
    {
        public static string SinceDate(this DateTime dateTime)
        {
            return dateTime.ToLongDateString();
        }

        public static IEnumerable<DateTime> DaysInMonth(this DateTime dateTime)
        {
            var firstDayOfMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            return DaysInPeriod(firstDayOfMonth, lastDayOfMonth);
        }

        public static IEnumerable<DateTime> DaysInPeriod(DateTime start, DateTime end)
        {
            return Enumerable.Range(0, 1 + end.Subtract(start).Days)
              .Select(offset => start.AddDays(offset))
              .ToArray();
        }
    }
}
