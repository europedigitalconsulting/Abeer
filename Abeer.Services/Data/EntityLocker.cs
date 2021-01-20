using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Abeer.Services.Data
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
