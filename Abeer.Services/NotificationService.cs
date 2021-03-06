using Abeer.Data.UnitOfworks;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task Create(Notification notification)
        {
            await _UnitOfWork.NotificationRepository.Add(notification);
        }

        public async Task Create(string userId, string title, string notificationUrl)
        {
            await Create(new Notification
            {
                UserId = userId,
                CssClass = "alert-info",
                DisplayMax = 1,
                ImageUrl = "alert-info",
                IsDisplayOnlyOnce = true,
                MessageTitle = title,
                NotificationUrl = notificationUrl, 
                NotificationIcon  = "alert-info", 
                NotificationType = "alert-info"
            });
        }

        public async Task Create(string userId, string title, string notificationUrl, string cssClass)
        {
            await Create(new Notification
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

        public async Task Create(string userId, string title, string notificationUrl, string cssClass, string imageUrl)
        {
            await Create(new Notification
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

        public async Task Create(string userId, string title, string notificationUrl, string cssClass, string imageUrl, string notificationIcon)
        {
            await Create(new Notification
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

        public async Task Create(string userId, string title, string notificationUrl, string cssClass, string imageUrl, string notificationIcon, string notificationType)
        {
            await Create(new Notification
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

        public async Task<IList<Notification>> GetNotifications() =>
            await _UnitOfWork.NotificationRepository.GetNotifications();

        public async Task<Notification> GetNotification(string userId, string type)
        {
            return await _UnitOfWork.NotificationRepository.FirstOrDefault(n => n.UserId == userId && n.NotificationType == type);
        }
    }
}
