using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Services.EmailConfirmation.Impl
{
    public class DevEmailSender : IEmailSender
    {
        private readonly ILogger<DevEmailSender> _logger;

        public DevEmailSender(ILogger<DevEmailSender> logger)
        {
            _logger = logger;
        }
        
        public async Task SendEmailAsync(string emailTo, string subject, string message)
        {
            var logMessage = $@"========
Sending email to {emailTo}
Subject: {subject}
Message: {message}
========";
            
            
            _logger.LogInformation(logMessage);
            await Task.CompletedTask;
        }
        
    }
}
