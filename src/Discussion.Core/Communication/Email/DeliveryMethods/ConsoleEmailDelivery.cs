using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Communication.Email.DeliveryMethods
{
    public class ConsoleEmailDelivery : IEmailDeliveryMethod
    {
        private readonly ILogger<ConsoleEmailDelivery> _logger;

        public ConsoleEmailDelivery(ILogger<ConsoleEmailDelivery> logger)
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
