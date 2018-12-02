using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Core.Cryptography
{
    public class DataProtectionOptions
    {
        public bool DisableAutomaticKeyGeneration { get; set; }
        public string KeyRepositoryPath { get; set; }
        public string ApplicationName { get; set; }
    }

    public static class DataProtectionServiceExtensions
    {
        public static void ConfigureDataProtection(this IServiceCollection services, IConfiguration appConfiguration)
        {
            var optionsSection = appConfiguration.GetSection(nameof(DataProtectionOptions));
            var options = optionsSection?.Get<DataProtectionOptions>();
            if (options == null)
            {
                return;
            }

            var dataProtection = services.AddDataProtection();
            if (!string.IsNullOrEmpty(options.ApplicationName))
            {
                dataProtection.SetApplicationName(options.ApplicationName);
            }

            if (!string.IsNullOrEmpty(options.KeyRepositoryPath))
            {
                dataProtection.PersistKeysToFileSystem(new DirectoryInfo(options.KeyRepositoryPath));
            }

            if (options.DisableAutomaticKeyGeneration)
            {
                dataProtection.DisableAutomaticKeyGeneration();
            }
        }
    }
}