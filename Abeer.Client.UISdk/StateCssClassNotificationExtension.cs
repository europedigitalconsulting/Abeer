using Abeer.Shared;

namespace Abeer.Client
{
    public static class StateCssClassNotificationExtension
    {
        public static string StateCssClass(this Notification notification)
        {
            return "unread";
        }
    }
}
