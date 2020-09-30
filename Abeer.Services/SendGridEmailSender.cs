using Abeer.Services.Options;

using Microsoft.Extensions.Logging;

using SendGrid;
using SendGrid.Helpers.Mail;

using System;
using System.Net;
using System.Threading.Tasks;

namespace Abeer.Services
{
    public class SendGridEmailSender : IEmailSenderService
    {
        private readonly ILogger<SendGridEmailSender> logger;

        public SendGridEmailSender(SendGridOptions sendGridOptions, ILogger<SendGridEmailSender> logger)
        {
            Options = sendGridOptions;
            this.logger = logger;
        }

        public SendGridOptions Options { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            logger.LogInformation($"start send email {subject} to {email}");
            return Execute(Options.SendGridApiKey, subject, message, email);
        }

        public async Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);

            var msg = new SendGridMessage()
            {
                From = new EmailAddress(Options.SendGridUser, Options.MailFromDisplayName),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(email));
            msg.SetClickTracking(true, true);

            var response = await client.SendEmailAsync(msg);

            logger.LogInformation($"send email {subject} to {email} result {response.StatusCode}");

            if (response.StatusCode != HttpStatusCode.Accepted)
                throw new ApplicationException("can not send sendgrid message");
        }
    }
}