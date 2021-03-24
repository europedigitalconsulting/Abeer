using System;
using AutoMapper;
using Abeer.Ads.Data.Repositories;
using Abeer.Ads.Models;

namespace Abeer.Ads.Data
{
    public class AdsUnitOfWork
    {
        private readonly IMapper _mapper = 
                new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<AutoMapping>();
                }).CreateMapper();

        public AdsUnitOfWork(AdsContext adsContext)
        {
            AdsContext = adsContext;
        }

        private IServiceProvider ServiceProvider { get; }
        public AdsContext AdsContext { get; }

        public AdsFamiliesRepository FamiliesRepository => new AdsFamiliesRepository(AdsContext, _mapper);
        public AdsCategoriesRepository CategoriesRepository => new AdsCategoriesRepository(AdsContext, _mapper);

        public AdsFamilyAttributesRepository FamilyAttributesRepository => new AdsFamilyAttributesRepository(AdsContext, _mapper);
   }
}
