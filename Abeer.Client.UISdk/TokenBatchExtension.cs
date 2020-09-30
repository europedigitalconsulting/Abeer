using Abeer.Shared;
using System.Linq;

namespace Abeer.Client
{
    public static class TokenBatchExtension
    {
        public static bool IsFullyUsed(this TokenBatch tokenBatch) => !tokenBatch.TokenItems.Any(t => !t.IsUsed);
        public static int FreeToken(this TokenBatch tokenBatch) => tokenBatch.TokenItems.Count(t => !t.IsUsed);
        public static decimal Percent(this int value, decimal max) => value / max;
    }
}
