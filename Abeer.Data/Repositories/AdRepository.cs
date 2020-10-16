﻿using Abeer.Shared.Functional;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class AdRepository
    {
        private readonly IFunctionalDbContext _context;
        public AdRepository(IFunctionalDbContext context)
        {
            _context = context;
        }

        public async Task<AdModel> AddAsync(AdModel current)
        {
            var entity = await _context.Ads.AddAsync(current);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public async Task<IEnumerable<AdModel>> GetAllForAUser(string userId)
        {
            return await _context.Ads.Include(a => a.AdPrice).Where(o => o.OwnerId == userId).ToListAsync();
        }

        public async Task Update(AdModel ad)
        {
            _context.Ads.Update(ad);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var ad = await _context.Ads.FirstOrDefaultAsync(a => a.Id == id);
            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AdModel>> GetVisibled()
        {
            return await _context.Ads.Include(a => a.AdPrice).Where(a => a.StartDisplayTime <= DateTime.UtcNow && a.EndDisplayTime >= DateTime.UtcNow
                && a.IsValid == true).ToListAsync();
        }

        public async Task<AdModel> FirstOrDefaultAsync(Expression<Func<AdModel, bool>> p)
        {
            return await _context.Ads.Include(a=>a.AdPrice).FirstOrDefaultAsync(p);
        }

        public async Task<List<AdModel>> AllAsync()
        {
            return await _context.Ads.Include(a => a.AdPrice).OrderByDescending(a => a.CreateDate).ToListAsync();
        }
    }
}