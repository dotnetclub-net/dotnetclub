using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Logging
{
    public static class FileLoggingExtensions
    {
        public static void AddSeriFileLogger(this ILoggingBuilder logging, IConfiguration configuration)
        {
            configuration = configuration?.GetSection("File");
            if (configuration == null)
            {
                return;
            }

            var seriConfigType = typeof(FileLoggerExtensions).Assembly.GetType("Serilog.Extensions.Logging.File.FileLoggingConfiguration");
            var seriFileLoggingConfig = configuration.Get(seriConfigType);
            if (seriFileLoggingConfig == null)
            {
                return;
            }
            
            logging.AddFile(configuration); // See doc at https://github.com/serilog/serilog-extensions-logging-file
        }
    }
}