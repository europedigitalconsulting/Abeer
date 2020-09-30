
using Abeer.Services.Options;

using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Services
{
    public class SmtpEmailSender : IEmailSenderService
    {
        public SmtpEmailSender(SmtpOptions smtpOptions)
        {
            Options = smtpOptions;
        }

        public SmtpOptions Options { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            SmtpClient smtpClient;

            if (!string.IsNullOrWhiteSpace(Options.ServerHost))
            {
                smtpClient = new SmtpClient(Options.ServerHost, Options.Port);
            }
            else
            {
                smtpClient = new SmtpClient();
            }

            if (!string.IsNullOrWhiteSpace(Options.UserId))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(Options.UserId, Options.Password);
            }
            else
            {
                smtpClient.UseDefaultCredentials = true;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(Options.MailFromAddress, Options.MailFromDisplayName),
                Body = message,
                BodyEncoding = new UTF8Encoding(),
                IsBodyHtml = true,
                Subject = subject
            };

            mailMessage.To.Add(email);

            smtpClient.Send(mailMessage);

            return Task.CompletedTask;
        }
    }
}