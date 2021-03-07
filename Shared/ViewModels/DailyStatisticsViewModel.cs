using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.ViewModels
{
    public class DailyStatisticsViewModel
    {
        public UserStatisticsViewModel UserStatistics { get; set; }
        public AdsStatisticsViewModel AdsStatistics { get; set; }
        public OwnerStatisticsViewModel OwnerStatistics { get; set; }

    }
}
