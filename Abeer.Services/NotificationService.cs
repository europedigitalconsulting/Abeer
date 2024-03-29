﻿using Abeer.Data.UnitOfworks;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
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

        public async Task<IList<Notification>> GetNotifications(string userId, NotificationTypeEnum notificationType) =>
            await _UnitOfWork.NotificationRepository.GetNotifications(userId, notificationType.GetName());

        public async Task<IList<Notification>> GetNotifications(string ownerId, NotificationTypeEnum notificationType, DateTime createdDate, string callbackUrl) =>
            await _UnitOfWork.NotificationRepository.Where(a => a.UserId == ownerId && a.NotificationType == NotificationTypeEnum.AdStartPublished.GetName() && a.CreatedDate > createdDate &&
                    a.NotificationUrl == callbackUrl);


        public async Task<Notification> Create(Notification notification)
        {
            return await _UnitOfWork.NotificationRepository.Add(notification);
        }

        public async Task Update(Notification notification)
        {
            await _UnitOfWork.NotificationRepository.Update(notification);
        }

        public async Task<Notification> Create(string userId, string title, string notificationUrl, string cssClass, string imageUrl, string notificationIcon, NotificationTypeEnum notificationType)
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
                NotificationType = notificationType.GetName()
            });
        }
    }
}
