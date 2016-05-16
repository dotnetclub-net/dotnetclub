using Discussion.Web.Data.InMemory;
using Jusfr.Persistent;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Startup;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;
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

                _applicationContext = BuildApplication( (c)=> { }, setupServices, (a, h, l) => { } );
                return _applicationContext;
            }
        }


        #region Proxy Context Properties

        public StubLoggerFactory LoggerFactory { get { return this.Context.LoggerFactory; } }
        public IApplicationEnvironment ApplicationEnvironment { get { return this.Context.ApplicationEnvironment; }  }
        public IHostingEnvironment HostingEnvironment { get { return this.Context.HostingEnvironment; }  }

        public RequestDelegate RequestHandler { get { return this.Context.RequestHandler; }  }
        public IServiceProvider ApplicationServices { get { return this.Context.ApplicationServices; }  }

        public IConfigurationRoot Configuration { get { return this.Context.Configuration; }  }
        public TestServer Server { get { return this.Context.Server; }  }

        #endregion


        public static TestApplicationContext BuildApplication(Action<IConfigurationBuilder> customConfiguration, Action<IServiceCollection> serviceConfiguration, Action<IApplicationBuilder, IHostingEnvironment, ILoggerFactory> serviceCustomConfiguration, string environmentName = "Production")
        {
            StartupMethods startup = null;
            IApplicationBuilder bootingupApp = null;
            var bootstrapDiagnosticMessages = new List<string>();


            var testApp = new TestApplicationContext
            {
                LoggerFactory = new StubLoggerFactory(),
                ApplicationEnvironment = CreateApplicationEnvironment(),
                HostingEnvironment = new HostingEnvironment { EnvironmentName = environmentName }
            };
            testApp.Configuration = BuildConfiguration(testApp.HostingEnvironment, testApp.ApplicationEnvironment, customConfiguration);


            Func<IServiceCollection, IServiceProvider> configureServices = services =>
            {
                services.AddInstance<ILoggerFactory>(testApp.LoggerFactory);
                services.AddInstance<IApplicationEnvironment>(testApp.ApplicationEnvironment);

                var loader = new StartupLoader(services.BuildServiceProvider(), testApp.HostingEnvironment);
                startup = loader.LoadMethods(typeof(Startup), bootstrapDiagnosticMessages);
                startup.ConfigureServicesDelegate(services);
                serviceConfiguration(services);

                testApp.ApplicationServices = services.BuildServiceProvider();
                return testApp.ApplicationServices;
            };
            Action<IApplicationBuilder> configure = app =>
            {
                bootingupApp = app;
                startup.ConfigureDelegate(app);
                serviceCustomConfiguration(app, testApp.HostingEnvironment, testApp.ApplicationServices.GetService<ILoggerFactory>());
            };


            var webHostBuilder = TestServer
                                .CreateBuilder(testApp.Configuration)
                                .UseEnvironment(environmentName)
                                .UseStartup(configure, configureServices);

            testApp.Server = new TestServer(webHostBuilder);
            testApp.RequestHandler = bootingupApp.Build();

            return testApp;
        }

        static IConfigurationRoot BuildConfiguration(IHostingEnvironment env, IApplicationEnvironment appEnv, Action<IConfigurationBuilder> customConfiguration)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(appEnv.ApplicationBasePath)
              .AddJsonFile("appsettings.json", optional: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            customConfiguration(builder);

            return builder.Build();
        }

        internal static IApplicationEnvironment CreateApplicationEnvironment()
        {
            var env = new TestApplicationEnvironment();
            env.ApplicationBasePath = WebProjectPath();
            env.ApplicationName = "Discussion.Web";

            return env;
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
         IApplicationEnvironment ApplicationEnvironment { get;  }
         IHostingEnvironment HostingEnvironment { get;  }

         RequestDelegate RequestHandler { get;  }
         IServiceProvider ApplicationServices { get;  }

         IConfigurationRoot Configuration { get;  }
         TestServer Server { get;  }
    }

    public class TestApplicationContext: IApplicationContext
    {
        public StubLoggerFactory LoggerFactory { get; set; }
        public IApplicationEnvironment ApplicationEnvironment { get; set; }
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

    class TestApplicationEnvironment : IApplicationEnvironment
    {
        public string ApplicationBasePath { get; set; }

        public string ApplicationName { get; set; }

        public string ApplicationVersion => PlatformServices.Default.Application.ApplicationVersion;

        public string Configuration => PlatformServices.Default.Application.Configuration;

        public FrameworkName RuntimeFramework => PlatformServices.Default.Application.RuntimeFramework;

        public object GetData(string name)
        {
            return PlatformServices.Default.Application.GetData(name);
        }

        public void SetData(string name, object value)
        {
            PlatformServices.Default.Application.SetData(name, value);
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

            var actionBindingContext = GetActionBindingContext(services.GetService<IOptions<MvcOptions>>().Value, actionContext);
            services.GetRequiredService<IActionBindingContextAccessor>().ActionBindingContext = actionBindingContext;

            var controllerFactory = services.GetService<IControllerFactory>();
            return controllerFactory.CreateController(actionContext) as T;
        }

        private static ActionBindingContext GetActionBindingContext(MvcOptions options, ActionContext actionContext)
        {
            var valueProviderFactoryContext = new ValueProviderFactoryContext(actionContext.HttpContext, actionContext.RouteData.Values);
            var valueProvider = CompositeValueProvider.CreateAsync(options.ValueProviderFactories, valueProviderFactoryContext).Result;

            return new ActionBindingContext()
            {
                InputFormatters = options.InputFormatters,
                OutputFormatters = options.OutputFormatters,
                ValidatorProvider = new CompositeModelValidatorProvider(options.ModelValidatorProviders),
                ModelBinder = new CompositeModelBinder(options.ModelBinders),
                ValueProvider = valueProvider
            };
        }

        public static T GetService<T>(this IApplicationContext app) where T : class
        {
            return app.ApplicationServices.GetService<T>();
        }
    }
}
