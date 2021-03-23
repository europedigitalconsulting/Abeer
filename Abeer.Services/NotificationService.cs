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
    public class NotificationService
    {
        private readonly FunctionalUnitOfWork _UnitOfWork;

        public NotificationService(FunctionalUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<IList<Notification>> GetNotifications(string userId, bool isDisplayed) =>
            await _UnitOfWork.NotificationRepository.GetNotifications(userId, isDisplayed);

        public async Task<IList<Notification>> GetNotifications(string userId, string notificationType) =>
            await _UnitOfWork.NotificationRepository.GetNotifications(userId, notificationType);

        public async Task<Notification> Create(Notification notification)
        {
            return await _UnitOfWork.NotificationRepository.Add(notification);
        }

        public async Task<Notification> Create(string userId, string title, string notificationUrl)
        {
            return await Create(new Notification
            {
                UserId = userId,
                CssClass = "alert-info",
                DisplayMax = 1,
                ImageUrl = "alert-info",
                IsDisplayOnlyOnce = true,
                MessageTitle = title,
                NotificationUrl = notificationUrl,
                NotificationIcon = "alert-info",
                NotificationType = "alert-info"
            });
        }

        public async Task<Notification> Create(string userId, string title, string notificationUrl, string cssClass)
        {
            return await Create(new Notification
            {
                UserId = userId,
                CssClass = cssClass,
                DisplayMax = 1,
                ImageUrl = "alert-info",
                IsDisplayOnlyOnce = true,
                MessageTitle = title,
                NotificationUrl = notificationUrl,
                NotificationIcon = "alert-info",
                NotificationType = "alert-info"
            });
        }

        public async Task<Notification> Create(string userId, string title, string notificationUrl, string cssClass, string imageUrl)
        {
            return await Create(new Notification
            {
                UserId = userId,
                CssClass = cssClass,
                DisplayMax = 1,
                ImageUrl = imageUrl,
                IsDisplayOnlyOnce = true,
                MessageTitle = title,
                NotificationUrl = notificationUrl,
                NotificationIcon = "alert-info",
                NotificationType = "alert-info"
            });
        }

        public async Task Update(Notification notification)
        {
            await _UnitOfWork.NotificationRepository.Update(notification);
        }

        public async Task<Notification> Create(string userId, string title, string notificationUrl, string cssClass, string imageUrl, string notificationIcon)
        {
            return await Create(new Notification
            {
                UserId = userId,
                CssClass = cssClass,
                DisplayMax = 1,
                ImageUrl = imageUrl,
                IsDisplayOnlyOnce = true,
                MessageTitle = title,
                NotificationUrl = notificationUrl,
                NotificationIcon = notificationIcon,
                NotificationType = "alert-info"
            });
        }

        public Task<IList<Notification>> Where(Expression<Func<Notification, bool>> filter) => _UnitOfWork.NotificationRepository.Where(filter);

        public async Task<Notification> Create(string userId, string title, string notificationUrl, string cssClass, string imageUrl, string notificationIcon, string notificationType)
        {
            return await Create(new Notification
            {
                UserId = userId,
                CssClass = cssClass,
                DisplayMax = 1,
                ImageUrl = imageUrl,
                IsDisplayOnlyOnce = true,
                MessageTitle = title,
                NotificationUrl = notificationUrl,
                NotificationIcon = notificationIcon,
                NotificationType = notificationType
            });
        }
    }
}
