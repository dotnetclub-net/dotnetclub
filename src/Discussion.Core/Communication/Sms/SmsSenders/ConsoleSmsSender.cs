using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Communication.Sms.SmsSenders
{
    public class ConsoleSmsSender: ISmsSender
    {       
        private readonly ILogger<ConsoleSmsSender> _logger;

        public ConsoleSmsSender(ILogger<ConsoleSmsSender> logger)
        {
            _logger = logger;
        }
        
        public async Task SendVerificationCodeAsync(string phoneNumber, string code)
        {
            var logMessage = $@"========
Sending SMS to {phoneNumber}
Message: verification code is {code}
========";
            _logger.LogInformation(logMessage);
            await Task.CompletedTask;
        }
    }
}