using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Web.Services.ChatHistoryImporting
{
    public static class ServiceExtensions
    {
        public static void AddChatyImportingServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            services.AddScoped<IChatHistoryImporter, DefaultChatHistoryImporter>();
            var chatyConfig = appConfiguration.GetSection(nameof(ChatyOptions));
            if (chatyConfig != null && !String.IsNullOrEmpty(chatyConfig[nameof(ChatyOptions.ServiceBaseUrl)]))
            {
                services.Configure<ChatyOptions>(chatyConfig);
            }

            services.AddScoped<ChatyApiService>();
        }
    }
}