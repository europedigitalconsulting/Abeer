using Abeer.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DbProvider.EfCore.SqlServerProvider
{
    internal class SqlServerSet<T> : IDbSet<T> where T : class
    {
        private DbSet<T> dbSets;
        private readonly DbContext sqlContext;

        public SqlServerSet(DbSet<T> dbSets, DbContext sqlContext)
        {
            this.dbSets = dbSets;
            this.sqlContext = sqlContext;
        }

        public T Add(T entity)
        {
            this.dbSets.Add(entity);
            sqlContext.SaveChanges();
            return entity;
        }

        public bool Any(Expression<Func<T, bool>> p)
        {
            return dbSets.Any(p);
        }

        public int Count()
        {
            return dbSets.Count();
        }

        public int Count(Expression<Func<T, bool>> p)
        {
            return dbSets.Count(p);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> p)
        {
            return dbSets.FirstOrDefault(p);
        }

        public void Remove(object id)
        {
            var entity = dbSets.Find(id);
    
            if(entity != null)
                dbSets.Remove(entity);

            sqlContext.SaveChanges();
        }

        public IList<T> ToList()
        {
            return dbSets.ToList();
        }

        public T Update(T entity)
        {
            dbSets.Update(entity);
            sqlContext.SaveChanges();
            return entity;
        }

        public IList<T> Where(Expression<Func<T, bool>> p, int skip = 0, int limit = int.MaxValue)
        {
            var query = dbSets.Where(p);

            if (skip > 0)
                query = query.Skip(skip);

            if (limit > 0)
                query = query.Take(limit);

            return query.ToList();
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            dbSets.AddRange(entities);
            sqlContext.SaveChanges();
            return entities;
        }

        public IQueryable<T> AsQuery() => dbSets.AsQueryable();
    }
}