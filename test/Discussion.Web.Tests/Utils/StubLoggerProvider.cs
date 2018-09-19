using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Tests
{
    public class StubLoggerProvider : ILoggerProvider, IDisposable
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new Logger { Provider = this, Category = categoryName };
        }

        public ConcurrentStack<LogItem> LogItems { get; private set; } = new ConcurrentStack<LogItem>();

        public void Dispose()
        {
            LogItems.Clear();
            LogItems = null;
        }

        public class Logger : ILogger
        {
            private class NoopDisposable : IDisposable
            {
                public static Logger.NoopDisposable Instance = new Logger.NoopDisposable();

                public void Dispose()
                {
                }
            }

            public StubLoggerProvider Provider { get; set; }
            public string Category { get; set; }

            public IDisposable BeginScope<TState>(TState state)
            {
                return NoopDisposable.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                var log = new LogItem
                {
                    Category = this.Category,
                    Level = logLevel,
                    EventId = eventId,
                    State = state,
                    Exception = exception,
                    Message = formatter.Invoke(state, exception)
                };
                Provider.LogItems.Push(log);
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var log = new LogItem
                {
                    Category = this.Category,
                    Level = logLevel,
                    EventId = eventId.Id,
                    State = state,
                    Exception = exception,
                    Message = formatter.Invoke(state, exception)
                };
                Provider.LogItems.Push(log);
            }
        }

        public class LogItem
        {
            public string Category { get; set; }
            public LogLevel Level { get; set; }
            public Exception Exception { get; set; }
            public int EventId { get; set; }
            public object State { get; set; }
            public string Message { get; set; }
        }
    }
}