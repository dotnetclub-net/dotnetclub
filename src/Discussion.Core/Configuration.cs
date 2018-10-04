using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Discussion.Core
{
    public static class Configuration
    {
        public const string ConfigKeyLogging = "Logging";
        private const string EVariablePrefixHosting = "ASPNETCORE_";
        private const string EVariablePrefixAppSettings = "DOTNETCLUB_";
        
        public static IWebHostBuilder ConfigureHost(IWebHostBuilder hostBuilder, bool addCommandLineArguments = false)
        {
            var basePath = Directory.GetCurrentDirectory();
            var hostSettingsBuilder = BuildHostConfiguration(basePath, addCommandLineArguments ? Environment.GetCommandLineArgs() : null);

            return hostBuilder
                .UseContentRoot(basePath)
                .UseConfiguration(hostSettingsBuilder.Build())
                .UseIISIntegration()
                .UseKestrel()
                .ConfigureAppConfiguration(BuildApplicationConfiguration)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                    ConfigureFileLogging(logging,
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

        public static void ConfigureFileLogging(ILoggingBuilder logging, IConfiguration loggingConfiguration, bool isDevelopment)
        {
            if (isDevelopment)
            {
                // Default is Information
                // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1
                logging.SetMinimumLevel(LogLevel.Trace);
            }

            var seriConfigType = typeof(FileLoggerExtensions).Assembly.GetType("Serilog.Extensions.Logging.File.FileLoggingConfiguration");
            var fileLoggingConfig = loggingConfiguration?.Get(seriConfigType);
            if (fileLoggingConfig != null)
            {
                logging.AddFile(loggingConfiguration); // See doc at nghttps://github.com/serilog/serilog-extensions-logging-file
            }
            logging.AddConfiguration(loggingConfiguration);
        }

    }
}