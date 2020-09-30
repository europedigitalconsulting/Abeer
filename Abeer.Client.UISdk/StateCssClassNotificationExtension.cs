using Abeer.Shared;
using System.Threading.Tasks;

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
