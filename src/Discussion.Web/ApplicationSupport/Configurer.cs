using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.ApplicationSupport
{
    public static class Configurer
    {
        internal const string ConfigKey_Logging = "Logging";
        const string EVariablePrefix_Hosting = "ASPNETCORE_";
        const string EVariablePrefix_AppSettings = "DOTNETCLUB_";
        
        public static IWebHostBuilder ConfigureHost(IWebHostBuilder hostBuilder, bool addCommandLineArguments = false)
        {
            var basePath = Directory.GetCurrentDirectory();
            var hostSettingsBuilder = BuildHostingSettings(basePath, addCommandLineArguments ? Environment.GetCommandLineArgs() : null);

            return hostBuilder
                .UseContentRoot(basePath)
                .UseConfiguration(hostSettingsBuilder.Build())
                .UseIISIntegration()
                .UseKestrel()
                .ConfigureAppConfiguration(ConfigureApplication)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                    ConfigureFileLogging(logging, 
                        hostingContext.HostingEnvironment.IsDevelopment(), 
                        hostingContext.Configuration.GetSection(ConfigKey_Logging));
                })
                .UseStartup<Startup>();
        }

        public static void ConfigureFileLogging(ILoggingBuilder logging, bool isDevelopment, IConfiguration loggingConfiguration)
        {
            if (isDevelopment)
            {
                // Default is Information
                // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1
                logging.SetMinimumLevel(LogLevel.Trace);
            }
            
            logging.AddFile(loggingConfiguration); // See doc at nghttps://github.com/serilog/serilog-extensions-logging-file
            logging.AddConfiguration(loggingConfiguration);
        }


        static IConfigurationBuilder BuildHostingSettings(string basePath, string[] commandlineArgs = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("hosting.json", optional: true)
                .AddEnvironmentVariables(prefix: EVariablePrefix_Hosting);

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

        static void ConfigureApplication(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var env = hostingContext.HostingEnvironment;
            config.SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(prefix: EVariablePrefix_AppSettings);
        }

       
    }
}