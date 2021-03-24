using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace Abeer.Shared.ViewModels
{
    public class DashboardInformation
    {
        public int NbVisits { get; set; }
        public int NbRegistar { get; set; }
        public int NbLogin { get; set; }
        public int NbRequestSubscription { get; set; }
        public int NbFinishSubscription { get; set; }
        public int NbAds { get; set; }
        public List<CommingFromSocialNetwork> CommingFromSocialNetworks { get; set; } = new List<CommingFromSocialNetwork>();
        public List<StatisticsDay> StatisticsDays { get; set; } = new List<StatisticsDay>();
    }

    public class CommingFromSocialNetwork
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class StatisticsDay
    {
        public DateTime Date { get; set; }
        public int NbVisits { get; set; }
        public int NbRegistar { get; set; }
        public int NbLogin { get; set; }
        public int NbRequestSubscription { get; set; }
        public int NbFinishSubscription { get; set; }
        public int NbAds { get; set; }
    }
}
