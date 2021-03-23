using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using AutoMapper;
using Abeer.Ads.Models; 
using Abeer.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace Abeer.Ads.Data.Repositories
{
    public class AdsCategoriesRepository : Repository<AdsCategoryViewModel, AdsCategory, Guid>
    {
        private readonly AdsContext _context;

        public AdsCategoriesRepository(AdsContext context, IMapper mapper) :
            base(context.Categories, (vm) => context.Categories.FirstOrDefault(c=>c.CategoryId == vm.CategoryId),
                categoryId => context.Categories.FirstOrDefault(c=>c.CategoryId == categoryId), mapper)
        {
            _context = context;
        }

        protected override void SaveChange()
        {
            _context.SaveChange();
        }

        public override Task<AdsCategoryViewModel> Get(Guid id)
        {
            return Task.Run(() =>
            {
                var model = _context.Categories.AsQuery().Include(c => c.Family).FirstOrDefault(c => c.CategoryId == id);
                return _mapper.Map<AdsCategoryViewModel>(model);
            });
        }

        public override Task<AdsCategoryViewModel> Update(AdsCategoryViewModel vm)
        {
            return Task.Run(() =>
            {
                var category = _context.Categories.FirstOrDefault(f => f.CategoryId == vm.CategoryId);

                category.Code = vm.Code;
                category.Label = vm.Label;
                category.MetaDescription = vm.MetaDescription;
                category.MetaKeywords = vm.MetaKeywords;
                category.MetaTitle = vm.MetaTitle;
                category.PictureUrl = vm.PictureUrl;

                _context.Categories.Update(category);

                return _mapper.Map<AdsCategoryViewModel>(category);
            });
        }

        public Task<IList<AdsCategoryViewModel>> FilterByFamilies(List<Guid> familiesId) =>
            GetAll<AdsCategoryViewModel, AdsCategory>(() => _context.Categories.Where(f => familiesId.Contains(f.FamilyId)).ToList());

        public Task<IList<AdsCategoryViewModel>> FilterByIds(List<Guid> ids) =>
            GetAll<AdsCategoryViewModel, AdsCategory>(() => _context.Categories.Where(f => ids.Contains(f.CategoryId)).ToList());

        public Task<IList<AdsCategoryViewModel>> GetByFamily(Guid familyId) =>
            GetAll<AdsCategoryViewModel, AdsCategory>(() => _context.Categories.Where(f => f.FamilyId == familyId).ToList());
    }
}
