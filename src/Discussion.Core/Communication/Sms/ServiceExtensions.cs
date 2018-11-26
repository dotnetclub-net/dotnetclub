using Discussion.Core.Communication.Sms.SmsSenders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Core.Communication.Sms
{
    public static class ServiceExtensions
    {
        public static void AddSmsServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            var smsConfigSection = appConfiguration.GetSection(nameof(AliyunSmsOptions));
            if (smsConfigSection != null && !string.IsNullOrEmpty(smsConfigSection[nameof(AliyunSmsOptions.AccountKeyId)]))
            {
                services.Configure<AliyunSmsOptions>(smsConfigSection);
                services.AddTransient<ISmsSender, AliyunSmsSender>();
            }
            else
            {
                services.AddTransient<ISmsSender, ConsoleSmsSender>();
            }
        }
    }
}