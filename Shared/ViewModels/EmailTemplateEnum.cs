using System;

namespace Abeer.Shared.ViewModels
{
    public enum EmailTemplateEnum
    {
        AdEndPublished = 0,
        AdSent = 1,
        AdStartPublished = 2,
        AdValidated = 3,
        ContactInvitationAdded = 4,
        ContactSent = 5,
        EmailConfirmation = 6,
        ExpiredProfile = 7,
        MessageReceived = 8,
        ForgotPassword = 9,
        RegisteredProfil = 10,
        SoonExpiredProfile = 11,
        SubscriptionPayed = 12
    }

    public static class EmailTemplateEnumExt
    {
        public static string GetName(this EmailTemplateEnum emailTemplateEnum)
        {
            return Enum.GetName(typeof(EmailTemplateEnum), emailTemplateEnum);
        }

        public static EmailTemplateEnum ConvertAsEmailType(this string name)
        {
            return Enum.Parse<EmailTemplateEnum>(name, true);
        }
    }
}
