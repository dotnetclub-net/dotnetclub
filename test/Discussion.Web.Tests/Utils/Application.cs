using Discussion.Web.Data.InMemory;
using Jusfr.Persistent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using Xunit;
using static Discussion.Web.Tests.TestEnv;

namespace Discussion.Web.Tests
{
    public sealed class Application: IApplicationContext, IDisposable
    {
        TestApplicationContext _applicationContext;
        InMemoryResponsitoryContext _dataContext;
        public TestApplicationContext Context
        {
            get
            {
                if(_applicationContext != null)
                {
                    return _applicationContext;
                }

                _applicationContext = BuildApplication();
                return _applicationContext;
            }
        }


        #region Proxy Context Properties

        public StubLoggerFactory LoggerFactory { get { return this.Context.LoggerFactory; } }
        public IHostingEnvironment HostingEnvironment { get { return this.Context.HostingEnvironment; }  }

        public RequestDelegate RequestHandler { get { return this.Context.RequestHandler; }  }
        public IServiceProvider ApplicationServices { get { return this.Context.ApplicationServices; }  }

        public TestServer Server { get { return this.Context.Server; }  }

        #endregion


        public static TestApplicationContext BuildApplication(string environmentName = "Production", Action<IWebHostBuilder> configureHost = null)
        {
            var testApp = new TestApplicationContext
            {
                LoggerFactory = new StubLoggerFactory()
            };


            var hostBuilder = new WebHostBuilder();
            if(configureHost != null)
            {
                configureHost(hostBuilder);
            }

            Startup.ConfigureHost(hostBuilder);

            hostBuilder.UseContentRoot(WebProjectPath())
                .UseEnvironment(environmentName)
                .UseLoggerFactory(testApp.LoggerFactory);

            testApp.Server = new TestServer(hostBuilder);
            testApp.RequestHandler = GetRequestHandler(testApp.Server.Host);
            testApp.ApplicationServices = testApp.Server.Host.Services;

            return testApp;
        }
        
        static RequestDelegate GetRequestHandler(IWebHost host)
        {
            var type = host.GetType();
            var field = type.GetField("_application", BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(host) as RequestDelegate;
        }

        #region Disposing

        ~Application()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }


            if (_dataContext != null)
            {
                (_dataContext as IDisposable).Dispose();
                _dataContext = null;
            }

            if(_applicationContext != null)
            {
                (_applicationContext as IDisposable).Dispose();
                _applicationContext = null;
            }
        }

        #endregion
    }

    // Use shared context to maintain database fixture
    // see https://xunit.github.io/docs/shared-context.html#collection-fixture
    [CollectionDefinition("AppSpecs")]
    public class ApplicationCollection : ICollectionFixture<Application>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public interface IApplicationContext: IDisposable
    {
         StubLoggerFactory LoggerFactory { get;  }
         IHostingEnvironment HostingEnvironment { get;  }

         RequestDelegate RequestHandler { get;  }
         IServiceProvider ApplicationServices { get;  }

         TestServer Server { get;  }
    }

    public class TestApplicationContext: IApplicationContext
    {
        public StubLoggerFactory LoggerFactory { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }        

        public RequestDelegate RequestHandler { get; set; }
        public IServiceProvider ApplicationServices { get; set; }

        public TestServer Server { get; set; }




        #region Disposing

        ~TestApplicationContext()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            RequestHandler = null;
            ApplicationServices = null;

            if (LoggerFactory != null)
            {
                LoggerFactory.LogItems.Clear();
                LoggerFactory = null;
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        #endregion
    }

    public class StubLoggerFactory : ILoggerFactory, IDisposable
    {
        public LogLevel MinimumLevel
        {
            get
            {
                return LogLevel.Debug;
            }

            set
            {

            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger { Factory = this };
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

            public StubLoggerFactory Factory { get; set; }

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
                    Level = logLevel,
                    EventId = eventId,
                    State = state,
                    Exception = exception,
                    Message = formatter.Invoke(state, exception)
                };
                Factory.LogItems.Push(log);
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var log = new LogItem
                {
                    Level = logLevel,
                    EventId = eventId.Id,
                    State = state,
                    Exception = exception,
                    Message = formatter.Invoke(state, exception)
                };
                Factory.LogItems.Push(log);
            }
        }

        public class LogItem
        {
            public LogLevel Level { get; set; }
            public Exception Exception { get; set; }
            public int EventId { get; set; }
            public object State { get; set; }
            public string Message { get; set; }
        }
    }

    public static class TestApplicationContextExtensions
    {
        public static T CreateController<T>(this IApplicationContext app) where T : Controller
        {
            var services = app.ApplicationServices;

            var actionContext = new ActionContext(
                new DefaultHttpContext
                {
                    RequestServices = services
                },
                new RouteData(),
                new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(T).GetTypeInfo()
                });

            // var actionBindingContext = GetActionBindingContext(services.GetService<IOptions<MvcOptions>>().Value, actionContext);
            // services.GetRequiredService<IActionBindingContextAccessor>().ActionBindingContext = actionBindingContext;

            var controllerFactory = services.GetService<IControllerFactory>();
            return controllerFactory.CreateController(new ControllerContext(actionContext)) as T;
        }

        //private static ActionBindingContext GetActionBindingContext(MvcOptions options, ActionContext actionContext)
        //{
        //    var valueProviderFactoryContext = new ValueProviderFactoryContext(actionContext.HttpContext, actionContext.RouteData.Values);
        //    var valueProvider = CompositeValueProvider.CreateAsync(options.ValueProviderFactories, valueProviderFactoryContext).Result;

        //    return new ActionBindingContext()
        //    {
        //        InputFormatters = options.InputFormatters,
        //        OutputFormatters = options.OutputFormatters,
        //        ValidatorProvider = new CompositeModelValidatorProvider(options.ModelValidatorProviders),
        //        ModelBinder = new CompositeModelBinder(options.ModelBinders),
        //        ValueProvider = valueProvider
        //    };



        //}
        

        public static T GetService<T>(this IApplicationContext app) where T : class
        {
            return app.ApplicationServices.GetService<T>();
        }
    }
}
