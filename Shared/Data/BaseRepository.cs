using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;

namespace Abeer.Shared.Data
{
    public abstract class BaseRepository
    {
        protected readonly IMapper _mapper;

        public BaseRepository(IMapper mapper)
        {
            _mapper = mapper;
        }
        protected Task<IList<TReturn>> GetAll<TReturn, TSource>(Func<IList<TSource>> getSource)
            where TReturn : class
        {
            var result = getSource();
            var data = _mapper.Map<IList<TSource>, IList<TReturn>>(result);
            return Task.FromResult(data);
        }

        protected Task<TVm> Add<TVm, TM>(TVm source, Func<TM, TM> addItem)
            where TVm : class
            where TM : class
        {
            return Task.Run(() =>
            {
                var model = _mapper.Map<TVm, TM>(source);
                var result = addItem(model);
                return _mapper.Map<TM, TVm>(result);
            });
        }

        protected Task<TVm> Update<TVm, TM>(TVm vm, Func<TVm, TM> getModel, Func<TM, TM> update)
            where TVm:class
            where TM:class
        {
            return Task.Run(() =>
            {
                var model = getModel(vm);
                _mapper.Map(vm, model);
                model = update(model);
                return _mapper.Map<TM, TVm>(model);
            });
        }
    }
}
