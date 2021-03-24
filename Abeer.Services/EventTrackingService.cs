using Abeer.Data.UnitOfworks;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Services
{
    public class EventTrackingService
    {
        private readonly FunctionalUnitOfWork _UnitOfWork;

        public EventTrackingService(FunctionalUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<IList<EventTrackingItem>> GetEventTrackingItems(string userId, string EventTrackingItemType) =>
            await _UnitOfWork.EventTrackingItemRepository.GetEventTrackingItems(userId, EventTrackingItemType);

        public async Task Create(EventTrackingItem EventTrackingItem)
        {
            await _UnitOfWork.EventTrackingItemRepository.Add(EventTrackingItem);
        }

        public async Task Create(string userId, string category, string key)
        {
            await Create(new EventTrackingItem
            {
                UserId = userId,
                Category = category,
                Key = key,
                CreatedDate = DateTime.UtcNow,
                Id = Guid.NewGuid()
            });
        }

        public async Task<IList<EventTrackingItem>> GetEventTrackingItems(string userId) =>
            (await _UnitOfWork.EventTrackingItemRepository.GetEventTrackingItems(userId))
                ?.OrderByDescending(n => n.CreatedDate).ToList();

        public async Task Update(EventTrackingItem EventTrackingItem)
        {
            await _UnitOfWork.EventTrackingItemRepository.Update(EventTrackingItem);
        }

        public async Task<IList<EventTrackingItem>> GetEventTrackingItems() =>
            (await _UnitOfWork.EventTrackingItemRepository.GetEventTrackingItems())
                ?.OrderByDescending(n => n.CreatedDate).ToList();


        public async Task<IList<EventTrackingItem>> GetEventTrackingItemByKey(string userId, string key)
        {
            return (await _UnitOfWork.EventTrackingItemRepository.Where(n => n.UserId == userId && n.Key == key))?
                .OrderByDescending(n => n.CreatedDate).ToList();
        }

        public async Task<EventTrackingItem> GetEventTrackingItemByCategory(string userId, string category)
        {
            return (await _UnitOfWork.EventTrackingItemRepository.Where(n => n.UserId == userId && n.Category == category))?
                .OrderByDescending(n => n.CreatedDate)
                .FirstOrDefault();
        }

        public async Task<IList<EventTrackingItem>> GetEventTrackingItemsByKey(string key) => await _UnitOfWork.EventTrackingItemRepository.GetEventTrackingItemsByKey(key);

        public async Task<IList<EventTrackingItem>> Where(Expression<Func<EventTrackingItem, bool>> filter) => await _UnitOfWork.EventTrackingItemRepository.Where(filter);

        public async Task<int> Count(Expression<Func<EventTrackingItem, bool>> filter) => await _UnitOfWork.EventTrackingItemRepository.Count(filter);
    }
}
