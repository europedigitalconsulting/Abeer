using System.Collections.Generic;
using Abeer.Shared.Data;
using Abeer.Ads.Models;

namespace Abeer.Ads.Data
{
    public class AdsContext
    {
        private readonly IDbProvider _dbProvider;

        public AdsContext(IDbProviderFactory dbProviderFactory)
        {
            _dbProvider = dbProviderFactory.GetProvider("Ads");
        }

        public IDbSet<AdsFamily> Families => _dbProvider.Set<AdsFamily>();
        public IDbSet<AdsCategory> Categories => _dbProvider.Set<AdsCategory>();
      public IDbSet<AdsFamilyAttribute> FamilyAttributes => _dbProvider.Set<AdsFamilyAttribute>();
      public IDbSet<CategoryAd> CategoryAds => _dbProvider.Set<CategoryAd>();

        public int SaveChange()
        {
            return _dbProvider.SaveChanges();
        }

        public void BulkUpdate<T>(IList<T> entities) where T : class
        {
            _dbProvider.BulkUpdate(entities);
        }

        public void BulkInsert<T>(IList<T> entities) where T : class
        {
            _dbProvider.BulkInsert(entities);
        }

        public void BulkDelete<T>(IList<T> entities) where T : class
        {
            _dbProvider.BulkDelete(entities);
        }

        public void EnsureCreated()
        {
            _dbProvider.EnsureCreated();
        }

        public void SetTimeout(int timeout)
        {
            _dbProvider.SetTimeout(timeout);
        }
    }
}
