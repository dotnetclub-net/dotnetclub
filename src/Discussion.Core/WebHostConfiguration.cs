using System;
using System.IO;
using System.Linq;
using Discussion.Core.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Discussion.Core
{
    public static class WebHostConfiguration
    {
        public const string ConfigKeyLogging = "Logging";
        private const string EVariablePrefixHosting = "ASPNETCORE_";
        private const string EVariablePrefixAppSettings = "DOTNETCLUB_";
        
        public static IWebHostBuilder Configure(IWebHostBuilder hostBuilder, bool addCommandLineArguments = false)
        {
            var basePath = Directory.GetCurrentDirectory();
            var hostSettingsBuilder = BuildHostConfiguration(basePath, addCommandLineArguments ? Environment.GetCommandLineArgs() : null);
            var hostConfiguration = hostSettingsBuilder.Build();

            return hostBuilder
                .UseContentRoot(basePath)
                .UseConfiguration(hostConfiguration)
                .UseIISIntegration()
                .UseKestrel(options => options.Configure(hostConfiguration.GetSection("Kestrel")))
                .ConfigureAppConfiguration(BuildApplicationConfiguration)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole(config => config.IncludeScopes = true);
                    FileLogging.Configure(logging,
                        hostingContext.Configuration.GetSection(ConfigKeyLogging),
                        hostingContext.HostingEnvironment.IsDevelopment());
                });
        }

        private static IConfigurationBuilder BuildHostConfiguration(string basePath, string[] commandlineArgs = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("hosting.json", optional: true)
                .AddEnvironmentVariables(prefix: EVariablePrefixHosting);

            if (commandlineArgs != null)
            {
                var args = Environment.GetCommandLineArgs();
                var firstArgs = args.FirstOrDefault(arg => arg.StartsWith("--"));
                var argsIndex = Array.IndexOf(args, firstArgs);

                if (argsIndex > -1)
                {
                    var usefulArgs = args.Skip(argsIndex).ToArray();
                    builder.AddCommandLine(usefulArgs);
                }
            }

            return builder;
        }

        private static void BuildApplicationConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var env = hostingContext.HostingEnvironment;
            config.SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(prefix: EVariablePrefixAppSettings);
        }
    }
}