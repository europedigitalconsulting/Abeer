using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using System.Linq;

namespace Abeer.Shared.Data
{
    public abstract class Repository<TVm, TModel, TKey> : BaseRepository
        where TVm : class
        where TModel : class
    {
        protected readonly IDbSet<TModel> _dbSet;
        protected readonly Func<TKey, TModel> _getByIdWithKey;
        protected readonly Func<TVm, TModel> _getById;

        protected abstract void SaveChange();

        public Repository(IDbSet<TModel> dbSet, Func<TVm, TModel> getById, Func<TKey, TModel> getByIdWithKey, IMapper mapper):base(mapper)
        {
            _dbSet = dbSet;
            _getByIdWithKey = getByIdWithKey;
            _getById = getById;
        }

        public virtual Task<IList<TVm>> GetAll() =>
            GetAll<TVm, TModel>(() => _dbSet.ToList());

        public abstract Task<TVm> Get(TKey id);

        public virtual Task<TVm> Add(TVm vm)
            => Add<TVm, TModel>(vm,
                (model) =>
                {
                    var vm = _dbSet.Add(model);
                    SaveChange();
                    return vm;
                });

        public virtual Task<TVm> Update(TVm vm) =>
            Update(vm,
                v => _getById(vm), (item) =>
                {
                    _dbSet.Update(item);
                    SaveChange();
                    return item;
                });

        public virtual Task Remove(Guid id)
        {
            return Task.Run(() => _dbSet.Remove(id));
        }

        public virtual Task RemoveAll(List<Guid> id)
        {
            return Task.Run(() => _dbSet.RemoveAll(id));
        }

    }
}