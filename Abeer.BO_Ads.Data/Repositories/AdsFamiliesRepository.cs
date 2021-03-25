using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using AutoMapper;
using Abeer.Ads.Models;
using Abeer.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace Abeer.Ads.Data.Repositories
{
    public class AdsFamiliesRepository : Repository<AdsFamilyViewModel, AdsFamily, Guid>
    {
        private readonly AdsContext _context;

        public AdsFamiliesRepository(AdsContext context, IMapper mapper) :
            base(context.Families, vm => context.Families.FirstOrDefault(f=>f.FamilyId == vm.FamilyId),
                familyId =>context.Families.FirstOrDefault(f=>f.FamilyId == familyId), mapper)
        {
            _context = context;
        }

        public override Task<IList<AdsFamilyViewModel>> GetAll()
        {
            return GetAll<AdsFamilyViewModel, AdsFamily>(() =>
            {
                var data = _dbSet.AsQuery().Include(f => f.Attributes).Include(f => f.Categories).ToList();
                return data;
            });
        }

        public override Task<AdsFamilyViewModel> Get(Guid id)
        {
            return Task.Run(()=>_mapper.Map<AdsFamily, AdsFamilyViewModel>(_dbSet.AsQuery()
                .Include(f => f.Attributes)
                .FirstOrDefault(f => f.FamilyId == id)));
        }

        public override Task<AdsFamilyViewModel> Update(AdsFamilyViewModel vm)
        {
            return Task.Run(() =>
            {
                var mapper = new MapperConfiguration(cf =>
                    cf.CreateMap<AdsFamilyViewModel, AdsFamily>()
                        .ForMember(m => m.Attributes,
                            opt => opt.Ignore())).CreateMapper();

                var family = _dbSet.FirstOrDefault(f => f.FamilyId == vm.FamilyId);

                mapper.Map(vm, family);
                _dbSet.Update(family);

                return _mapper.Map<AdsFamily, AdsFamilyViewModel>(_dbSet.AsQuery()
                        .Include(f => f.Attributes)
                    .FirstOrDefault(f => f.FamilyId == vm.FamilyId));
            });
        }

        protected override void SaveChange()
        {
            _context.SaveChange();
        }
    }
}
