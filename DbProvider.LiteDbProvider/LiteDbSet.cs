using Abeer.Shared.Data;
using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DbProvider.LiteDbProvider
{
    internal class LiteDbSet<T> : IDbSet<T> where T : class
    {
        public LiteDbSet(LiteDatabase liteDatabase)
        {
            db = liteDatabase;
        }

        private LiteDatabase db;

        private TResult ExecuteReader<TResult>(Func<ILiteCollection<T>, TResult> function)
        {
            var col = db.GetCollection<T>(typeof(T).Name);
            return function(col);
        }

        private void ExecuteWriter(Action<ILiteCollection<T>> writing)
        {
            if (db.BeginTrans())
            {
                try
                {
                    var col = db.GetCollection<T>(typeof(T).Name);
                    writing(col);
                    db.Commit();
                }
                catch
                {
                    db.Rollback();
                    throw;
                }
            }
        }

        public T Add(T entity)
        {
            ExecuteWriter(col => col.Insert(entity));
            return entity;
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            ExecuteWriter(col => col.Insert(entities));
            return entities;
        }

        public bool Any(Expression<Func<T, bool>> p)
        {
            return ExecuteReader<bool>(col => col.Count(p) > 0);
        }

        public int Count()
        {
            return ExecuteReader<int>(col => col.Count());
        }

        public int Count(Expression<Func<T, bool>> p)
        {
            return ExecuteReader<int>(col => col.Count(p));
        }

        public T FirstOrDefault(Expression<Func<T, bool>> p)
        {
            return ExecuteReader<T>(col => col.FindOne(p));
        }

        public void Remove(object id)
        {
            ExecuteWriter(col => col.Delete(new BsonValue(id)));
        }

        public IList<T> ToList()
        {
            return ExecuteReader<List<T>>(col => col.FindAll()?.ToList());
        }

        public T Update(T entity)
        {
            ExecuteWriter(col =>
            {
                col.Update(entity);
            });

            return entity;
        }

        public IList<T> Where(Expression<Func<T, bool>> p, int skip = 0, int limit = int.MaxValue)
        {
            return ExecuteReader<IList<T>>(col => col.Find(p, skip, limit)?.ToList());
        }

        public IQueryable<T> AsQuery()
        {
            return ExecuteReader<IQueryable<T>>(col => col.FindAll().AsQueryable());
        }
    }
}