using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class CountriesRepository
    {
        private readonly IFunctionalDbContext _context;

        public CountriesRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        public Task<List<Country>> GetCountries(string culture)
        {
            return _context.Countries.Where(c=>c.Culture == culture).ToListAsync();
        }

        public Task<bool> AnyAsync(Expression<Func<Country, bool>> filter)
        {
            return _context.Countries.AnyAsync(filter);
        }

        public async Task<Country> AddAsync(Country country)
        {
            try
            {
                var result = await _context.Countries.AddAsync(country);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
