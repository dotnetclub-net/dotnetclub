using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Discussion.Tests.Common
{
    public class StubLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new Logger { Provider = this, Category = categoryName };
        }

        public ConcurrentStack<LogItem> LogItems { get; private set; } = new ConcurrentStack<LogItem>();
        
        private string CurrentScope { get; set; }

        public void Dispose()
        {
            LogItems.Clear();
            CurrentScope = null;
            LogItems = null;
        }

        public class Logger : ILogger
        {
            private class LogScope : IDisposable
            {
                private readonly StubLoggerProvider _provider;
                private readonly string _parentScope;
                public LogScope(StubLoggerProvider provider, string scope)
                {
                    _provider = provider;
                    _parentScope = provider.CurrentScope;
                    _provider.CurrentScope = scope;
                }

                public void Dispose()
                {
                    _provider.CurrentScope = _parentScope;
                }
            }

            public StubLoggerProvider Provider { get; set; }
            public string Category { get; set; }

            public IDisposable BeginScope<TState>(TState state)
            {
                return new LogScope(Provider, state.ToString());
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                var log = new LogItem
                {
                    Category = Category,
                    Level = logLevel,
                    EventId = eventId,
                    State = state,
                    Exception = exception,
                    Scope = Provider.CurrentScope,
                    Message = formatter.Invoke(state, exception)
                };
                Provider.LogItems.Push(log);
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var log = new LogItem
                {
                    Category = Category,
                    Level = logLevel,
                    EventId = eventId.Id,
                    State = state,
                    Exception = exception,
                    Scope = Provider.CurrentScope,
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
            
            public string Scope { get; set; }
            public string Message { get; set; }
        }
    }
}