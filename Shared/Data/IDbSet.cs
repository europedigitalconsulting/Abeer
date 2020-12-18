using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Abeer.Shared.Data
{
    public interface IDbSet<T> where T : class
    {
        T Add(T entity);
        IEnumerable<T> AddRange(IEnumerable<T> entities);
        T Update(T entity);
        T FirstOrDefault(Expression<Func<T, bool>> p);
        void Remove(object id);
        IList<T> ToList();
        bool Any(Expression<Func<T, bool>> p);
        IList<T> Where(Expression<Func<T, bool>> p, int skip = 0, int limit = int.MaxValue);
        int Count();
        int Count(Expression<Func<T, bool>> p);
    }
}
