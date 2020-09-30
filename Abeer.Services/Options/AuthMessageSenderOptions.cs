namespace Abeer.Services.Options
{
    public class AuthMessageSenderOptions
    {
        public string MailFromDisplayName { get; set; }
        public string MailFromAddress { get; set; }
    }

    public class SendGridOptions : AuthMessageSenderOptions
    {
        public string SendGridUser { get; set; }
        public string SendGridApiKey { get; set; }
    }

    public class PickFolderSmtpOptions : AuthMessageSenderOptions
    {
        public string PickFolderPath { get; set; }
    }

    public class SmtpOptions : AuthMessageSenderOptions
    {
        public int Port { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string ServerHost { get; internal set; }
    }
}
