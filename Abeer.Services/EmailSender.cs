using Abeer.Services.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace Abeer.Services
{
    public class EmailSenderFactory : IEmailSenderService
    {
        private IEmailSenderService _emailSender;

        public EmailSenderFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var emailType = configuration["EmailSender:EmailSenderType"].ToLower();
            
            if(emailType == "pickup")
            {
                var options = new PickFolderSmtpOptions();
                configuration.GetSection("EmailSender").Bind(options);
                _emailSender = new PickFolderEmailSender(options);
            }
            else if(emailType == "smtp")
            {
                var options = new SmtpOptions();
                configuration.GetSection("EmailSender").Bind(options);
                _emailSender = new SmtpEmailSender(options);
            }
            else
            {
                var options = new SendGridOptions();
                configuration.GetSection("EmailSender").Bind(options);
                var logger = ActivatorUtilities.GetServiceOrCreateInstance<ILogger<SendGridEmailSender>>(serviceProvider);
                _emailSender = new SendGridEmailSender(options, logger);
            }
        }

        public Task SendEmailAsync(string email, string subject, string message) => _emailSender.SendEmailAsync(email, subject, message);
    }
}