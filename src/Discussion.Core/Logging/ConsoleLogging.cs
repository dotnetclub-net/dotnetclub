using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Elasticsearch;

namespace Discussion.Core.Logging
{
    public static class ConsoleLogging
    {
        public static void AddSeriConsoleLogger(this ILoggingBuilder logging, IConfiguration configuration)
        {
            configuration = configuration?.GetSection("Console");
            var consoleConfig = configuration?.Get<ConsoleLoggingConfiguration>();
            if (consoleConfig == null)
            {
                return;
            }

            var minimumLogLevel = GetMinimumLogLevel(configuration);
            var levelOverrides = GetLevelOverrides(configuration);
            //  .WriteTo.Console(new CompactJsonFormatter())

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(MicrosoftToSerilogLevel(minimumLogLevel))
                .Enrich.FromLogContext()
                .WriteTo.Async(ConfigureConsoleSink(consoleConfig));

            foreach (var keyValuePair in levelOverrides)
            {
                loggerConfiguration.MinimumLevel
                    .Override(keyValuePair.Key, MicrosoftToSerilogLevel(keyValuePair.Value));
            }

            var consoleLogger = loggerConfiguration.CreateLogger();
            logging.AddSerilog(consoleLogger, true);
        }
        
        static Action<LoggerSinkConfiguration> ConfigureConsoleSink(ConsoleLoggingConfiguration consoleConfig)
        {
            if (!consoleConfig.Json)
            {
                return w => w.Console(LogEventLevel.Verbose, consoleConfig.OutputTemplate);
            }

            if (consoleConfig.ElasticSearchJson)
            {
                return w => w.Console(new ElasticsearchJsonFormatter());
            }

            return w => w.Console(new CompactJsonFormatter());

        }
        
        static LogLevel GetMinimumLogLevel(IConfiguration configuration)
        {
            var result = LogLevel.Information;
            var str = configuration["LogLevel:Default"];
            if (!string.IsNullOrWhiteSpace(str) && !Enum.TryParse(str, out result))
            {
                SelfLog.WriteLine("The minimum level setting `{0}` is invalid", (object) str, (object) null, (object) null);
                result = LogLevel.Information;
            }
            return result;
        }
        
        static LogEventLevel MicrosoftToSerilogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Critical:
                case LogLevel.None:
                    return LogEventLevel.Fatal;
                default:
                    return LogEventLevel.Verbose;
            }
        }
        
        static Dictionary<string, LogLevel> GetLevelOverrides(IConfiguration configuration)
        {
            var dictionary = new Dictionary<string, LogLevel>();
            foreach (IConfigurationSection configurationSection in configuration.GetSection("LogLevel").GetChildren().Where(cfg => cfg.Key != "Default"))
            {
                if (!Enum.TryParse<LogLevel>(configurationSection.Value, out var result))
                    SelfLog.WriteLine("The level override setting `{0}` for `{1}` is invalid", configurationSection.Value, configurationSection.Key);
                else
                    dictionary[configurationSection.Key] = result;
            }
            return dictionary;
        }
    }

    public class ConsoleLoggingConfiguration
    {
        const string DefaultOutputTemplate = "{Timestamp:o} {RequestId,13} [{Level:u3}] {Message} ({EventId:x8}){NewLine}{Exception}";
        public string OutputTemplate { get; set; } = DefaultOutputTemplate;
        public bool Json { get; set; }
        
        public bool ElasticSearchJson { get; set; }
    }
}