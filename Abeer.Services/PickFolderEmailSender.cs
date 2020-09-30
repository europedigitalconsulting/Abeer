
using Abeer.Services.Options;

using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Services
{
    public class PickFolderEmailSender : IEmailSenderService
    {
        public PickFolderEmailSender(PickFolderSmtpOptions pickFolderSmtpOptions)
        {
            Options = pickFolderSmtpOptions;
        }

        public PickFolderSmtpOptions Options { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var pickupFolder = new DirectoryInfo(Options.PickFolderPath).FullName;
            
            if (!Directory.Exists(pickupFolder))
                Directory.CreateDirectory(pickupFolder);

            using var smtpClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupFolder
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(Options.MailFromAddress, Options.MailFromDisplayName),
                Body = message, 
                BodyEncoding =  new UTF8Encoding(),
                IsBodyHtml=true,
                Subject = subject
            };

            mailMessage.To.Add(email);

            smtpClient.Send(mailMessage);

            return Task.CompletedTask;
        }
    }
}