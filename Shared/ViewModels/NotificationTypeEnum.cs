using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.ViewModels
{
    public enum NotificationTypeEnum
    {
        Welcome = 0,
        DailyReminder = 1,
        ExpiredProfile = 2,
        SoonExpireProfile = 3,
        AddContact = 4,
        AdStartPublished = 5,
        AdEndPublished = 6,
        RemoveContact = 7,
        SendProfile = 8,
        ValidateAd = 9
    }

    public static class NotificationTypeEnumExt
    {
        public static string GetName(this NotificationTypeEnum notificationTypeEnum)
        {
            return Enum.GetName(typeof(NotificationTypeEnum), notificationTypeEnum);
        }

        public static NotificationTypeEnum ConvertAsNotificationType(this string name)
        {
            return Enum.Parse<NotificationTypeEnum>(name, true);
        }
    }
}
