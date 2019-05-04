using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Logging
{
    public static class FileLoggingExtensions
    {
        public static void AddSeriFileLogger(this ILoggingBuilder logging, IConfiguration loggingConfiguration)
        {
            var seriConfigType = typeof(FileLoggerExtensions).Assembly.GetType("Serilog.Extensions.Logging.File.FileLoggingConfiguration");
            var fileLoggingConfig = loggingConfiguration?.Get(seriConfigType);
            if (fileLoggingConfig != null)
            {
                logging.AddFile(loggingConfiguration); // See doc at https://github.com/serilog/serilog-extensions-logging-file
            }
        }

    }
}