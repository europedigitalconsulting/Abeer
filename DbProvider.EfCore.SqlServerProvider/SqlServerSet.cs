using Abeer.Data;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DbProvider.EfCore.SqlServerProvider
{
    internal class SqlServerSet<T> : IDbSet<T> where T : class
    {
        private DbSet<T> dbSets;

        public SqlServerSet(DbSet<T> dbSets)
        {
            this.dbSets = dbSets;
        }

        public T Add(T entity)
        {
            throw new NotImplementedException();
        }

        public bool Any(Expression<Func<T, bool>> p)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public int Count(Expression<Func<T, bool>> p)
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> p)
        {
            throw new NotImplementedException();
        }

        public void Remove(object id)
        {
            throw new NotImplementedException();
        }

        public IList<T> ToList()
        {
            throw new NotImplementedException();
        }

        public T Update(T entity)
        {
            throw new NotImplementedException();
        }

        public IList<T> Where(Expression<Func<T, bool>> p, int skip = 0, int limit = int.MaxValue)
        {
            throw new NotImplementedException();
        }
    }
}