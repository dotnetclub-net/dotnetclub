using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Logging
{
    public static class FileLogging
    {
        public static void Configure(ILoggingBuilder logging, IConfiguration loggingConfiguration, bool isDevelopment)
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
                logging.AddFile(loggingConfiguration); // See doc at https://github.com/serilog/serilog-extensions-logging-file
            }
            logging.AddConfiguration(loggingConfiguration);
        }
    }
}