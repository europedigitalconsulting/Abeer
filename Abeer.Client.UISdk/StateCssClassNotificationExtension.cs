using Abeer.Shared;
using Abeer.Shared.Functional;

namespace Abeer.Client.UISdk
{
    public static class StateCssClassNotificationExtension
    {
        public static string StateCssClass(this Notification notification)
        {
            return "unread";
        }
    }
}
