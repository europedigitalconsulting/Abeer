using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.Data
{
    public interface IDbProvider
    {
        int SaveChanges();
        void BulkUpdate<T>(IList<T> entities) where T : class;
        void BulkInsert<T>(IList<T> entities) where T : class;
        void EnsureCreated();
        void SetTimeout(int timeout);
        IDbSet<T> Set<T>() where T : class;
    }
}
