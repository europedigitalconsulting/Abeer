using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.ViewModels
{
    public class OwnerStatisticsViewModel
    {
        public int ContactsCount { get; set; }
        public int InvitationSentCount { get; set; }
        public int InvitationReceivedCount { get; set; }
        public int AdsCount { get; set; }
    }
}
