using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using AutoMapper;
using Abeer.Ads.Models;
using Abeer.Shared.Data;

namespace Abeer.Ads.Data.Repositories
{
    public class AdsFamilyAttributesRepository : Repository<AdsFamilyAttributeViewModel, AdsFamilyAttribute, Guid>
    {
        private readonly AdsContext _context;

        public AdsFamilyAttributesRepository(AdsContext context, IMapper mapper) :
            base(context.FamilyAttributes, vm => context.FamilyAttributes.FirstOrDefault(a=>a.FamilyAttributeId == vm.FamilyAttributeId),
                familyAttributeId => 
                    context.FamilyAttributes.FirstOrDefault(fa=>fa.FamilyAttributeId == familyAttributeId), mapper)
        {
            _context = context;
        }

        protected override void SaveChange()
        {
            _context.SaveChange();
        }

        public override Task<AdsFamilyAttributeViewModel> Get(Guid id)
        {
            return Task.Run(()=>
                _mapper.Map<AdsFamilyAttribute, AdsFamilyAttributeViewModel>(
                _context.FamilyAttributes.FirstOrDefault(fa => fa.FamilyAttributeId == id)));
        }

        public Task<IList<AdsFamilyAttributeViewModel>> GetByFamily(Guid familyId) =>
            GetAll<AdsFamilyAttributeViewModel, AdsFamilyAttribute>(() => 
                _context.FamilyAttributes.Where(f => f.FamilyId == familyId).ToList());
    }
}
