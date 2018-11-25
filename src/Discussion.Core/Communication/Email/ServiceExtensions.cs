using Discussion.Core.Communication.Email.DeliveryMethods;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Core.Communication.Email
{
    public static class ServiceExtensions
    {
        public static void AddEmailServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            var configSection = appConfiguration.GetSection(nameof(EmailDeliveryOptions));
            if (configSection != null && !string.IsNullOrEmpty(configSection[nameof(EmailDeliveryOptions.ServerHost)]))
            {
                services.Configure<EmailDeliveryOptions>(configSection);
                services.AddTransient<IEmailDeliveryMethod, SmtpEmailEmailDelivery>();
            }
            else
            {
                services.AddTransient<IEmailDeliveryMethod, ConsoleEmailDelivery>();
            }
        }
    }
}