using Abeer.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class CountriesRepository
    {
        private readonly FunctionalDbContext _context;

        public CountriesRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<List<Country>> GetCountries(string culture)
        {
            return Task.Run(() => _context.Countries.Where(c=>c.Culture == culture).ToList());
        }

        public Task<bool> AnyAsync(Expression<Func<Country, bool>> filter)
        {
            return Task.Run(() => _context.Countries.Any(filter));
        }

        public bool Any(Expression<Func<Country, bool>> filter)
        {
            return _context.Countries.Any(filter);
        }

        public Country Add(Country country)
        {
            return _context.Countries.Add(country);
        }
    }
}
