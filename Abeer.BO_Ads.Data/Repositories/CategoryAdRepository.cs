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
    public class CategoryAdRepository : Repository<CategoryAdViewModel, CategoryAd, Guid>
    {
        private readonly AdsContext _context;

        public CategoryAdRepository(AdsContext context, IMapper mapper) :
            base(context.CategoryAds, (vm) => context.CategoryAds.FirstOrDefault(c => c.CategoryId == vm.CategoryId),
                categoryId => context.CategoryAds.FirstOrDefault(c => c.CategoryId == categoryId), mapper)
        {
            _context = context;
        }

        protected override void SaveChange()
        {
            _context.SaveChange();
        }
        public Task Add(List<Guid> listCategoryId, Guid adId)
        {
            RemoveListCategAd(adId);

            List<CategoryAd> list = new List<CategoryAd>();
            foreach (var item in listCategoryId)
            {
                CategoryAd model = new CategoryAd();
                model.AdId = adId;
                model.CategoryId = item;
                list.Add(model);
            }

            return Task.Run(() => _context.CategoryAds.AddRange(list));
        }
        public Task<List<Guid>> GetAllIdCatByAdId(Guid adId)
        {
            return Task.Run(() =>
            {
                var model = _context.CategoryAds.Where(c => c.AdId == adId).ToList().Select(x => x.CategoryId);
                return _mapper.Map<List<Guid>>(model);
            });
        }

        public override Task<CategoryAdViewModel> Get(Guid id)
        {
            return Task.Run(() =>
            {
                var model = _context.CategoryAds.FirstOrDefault(c => c.Id == id);
                return _mapper.Map<CategoryAdViewModel>(model);
            });
        }
        public  void RemoveListCategAd(Guid id)
        {
            var list = _context.CategoryAds.Where(x => x.AdId == id);
            _context.BulkDelete(list); 
        }

        public Task<IList<CategoryAd>> GetAllByCategoriesId(IEnumerable<Guid> categoriesId)
        {
            return Task.Run(() =>
            {
                return _context.CategoryAds.Where(c => categoriesId.Contains(c.CategoryId));
            });
        }

        //public Task<IList<CategoryAdViewModel>> FilterByFamilies(List<Guid> familiesId) =>
        //    GetAll<CategoryAdViewModel, AdsCategory>(() => _context.Categories.Where(f => familiesId.Contains(f.FamilyId)).ToList());

        //public Task<IList<CategoryAdViewModel>> FilterByIds(List<Guid> ids) =>
        //    GetAll<CategoryAdViewModel, AdsCategory>(() => _context.Categories.Where(f => ids.Contains(f.CategoryId)).ToList());

        //public Task<IList<CategoryAdViewModel>> GetByFamily(Guid familyId) =>
        //    GetAll<CategoryAdViewModel, AdsCategory>(() => _context.Categories.Where(f => f.FamilyId == familyId).ToList());
    }
}
