using Discussion.Web.Services.EmailConfirmation.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Web.Services.EmailConfirmation
{
    public static class ServiceExtensions
    {
        public static void AddEmailSendingServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            services.AddTransient<IConfirmationEmailBuilder, DefaultConfirmationEmailBuilder>();

            var configSection = appConfiguration.GetSection(nameof(EmailSendingOptions));
            if (configSection != null && !string.IsNullOrEmpty(configSection[nameof(EmailSendingOptions.ServerHost)]))
            {
                services.Configure<EmailSendingOptions>(appConfiguration);
                services.AddTransient<IEmailSender, SmtpEmailSender>();
            }
            else
            {
                services.AddTransient<IEmailSender, ConsoleEmailSender>();
            }
        }
    }
}