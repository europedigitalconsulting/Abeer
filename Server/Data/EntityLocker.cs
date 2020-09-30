using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Abeer.Server.Data
{
    public class EntityLocker
    {
        private static ConcurrentDictionary<string, string> keys = new ConcurrentDictionary<string, string>();
        public static void LockEntity(string name)
        {
            while (keys.ContainsKey(name))
                Task.Delay(100);

            keys.TryAdd(name, name);
        }

        public static void UnLockEntity(string name) =>
            keys.TryRemove(name, out _);
    }
}
