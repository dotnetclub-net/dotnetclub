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

                Action<IServiceCollection> setupServices = (services) =>
                {
                    _dataContext = new InMemoryResponsitoryContext();
                    services.AddScoped(typeof(IRepositoryContext), (serviceProvider) => _dataContext);
                    services.AddScoped(typeof(Repository<,>), typeof(InMemoryDataRepository<,>));
                };

                _applicationContext = BuildApplication( (c)=> { }, setupServices, (a) => { } );
                return _applicationContext;
            }
        }


        #region Proxy Context Properties

        public StubLoggerFactory LoggerFactory { get { return this.Context.LoggerFactory; } }
        public IHostingEnvironment HostingEnvironment { get { return this.Context.HostingEnvironment; }  }

        public RequestDelegate RequestHandler { get { return this.Context.RequestHandler; }  }
        public IServiceProvider ApplicationServices { get { return this.Context.ApplicationServices; }  }

        public IConfigurationRoot Configuration { get { return this.Context.Configuration; }  }
        public TestServer Server { get { return this.Context.Server; }  }

        #endregion


        public static TestApplicationContext BuildApplication(Action<IConfigurationBuilder> customConfiguration, Action<IServiceCollection> serviceConfiguration,  Action<IApplicationBuilder> appConfiguration,  string environmentName = "Production")
        {
            IApplicationBuilder bootingupApp = null;

            var testApp = new TestApplicationContext
            {
                LoggerFactory = new StubLoggerFactory()
            };


            var builder = new WebHostBuilder();
            HostingAbstractionsWebHostBuilderExtensions.UseContentRoot(builder, WebProjectPath());
            HostingAbstractionsWebHostBuilderExtensions.UseEnvironment(builder, environmentName);

            builder.Configure(app =>
            {
                appConfiguration(app);
                bootingupApp = app;
            })
            .ConfigureServices(serviceConfiguration)
            .UseLoggerFactory(testApp.LoggerFactory)
            .UseStartup<Startup>();
            
            testApp.Server = new TestServer(builder);
            testApp.RequestHandler = bootingupApp.Build();

            return testApp;
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

         IConfigurationRoot Configuration { get;  }
         TestServer Server { get;  }
    }

    public class TestApplicationContext: IApplicationContext
    {
        public StubLoggerFactory LoggerFactory { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }        

        public RequestDelegate RequestHandler { get; set; }
        public IServiceProvider ApplicationServices { get; set; }

        public IConfigurationRoot Configuration { get; set; }
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

    public class StubLoggerFactory : ILoggerFactory
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
            public StubLoggerFactory Factory { get; set; }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public IDisposable BeginScopeImpl(object state)
            {
                return null;
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
                throw new NotImplementedException();
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
