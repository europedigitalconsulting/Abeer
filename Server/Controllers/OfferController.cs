using Abeer.Shared.Functional;

using System.Collections.Concurrent;

namespace Abeer.Server.Controllers
{
    public class OfferController
    {
        private static readonly ConcurrentBag<OfferModel> Offers = new ConcurrentBag<OfferModel>();

    }
}
