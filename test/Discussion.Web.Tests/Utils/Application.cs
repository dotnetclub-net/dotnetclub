using Discussion.Web.Data.InMemory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using static Discussion.Web.Tests.TestEnv;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Discussion.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Discussion.Web.Tests
{
    public sealed class Application: IApplicationContext, IDisposable
    {

        ClaimsPrincipal _originalUser;
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

//                _applicationContext = BuildApplication("Development");
                _applicationContext = BuildApplication();
                _originalUser = _applicationContext.User;
                return _applicationContext;
            }
        }

        public Application Reset()
        {
            // reset all monifications in test cases
            User = _originalUser;
            return this;
        }

        #region Proxy Context Properties

        public StubLoggerProvider LoggerProvider { get { return this.Context.LoggerProvider; } }
        public IHostingEnvironment HostingEnvironment { get { return this.Context.HostingEnvironment; }  }

        public IServiceProvider ApplicationServices { get { return this.Context.ApplicationServices; }  }

        public TestServer Server { get { return this.Context.Server; }  }
        public ClaimsPrincipal User{ get { return this.Context.User; } set { this.Context.User = value; } }

        #endregion

        public static TestApplicationContext BuildApplication(string environmentName = "Production", Action<IWebHostBuilder> configureHost = null)
        {
            var testApp = new TestApplicationContext
            {
                LoggerProvider = new StubLoggerProvider(),
                User = new ClaimsPrincipal(new ClaimsIdentity())
            };

            var hostBuilder = new WebHostBuilder();
            if (configureHost != null)
            {
                configureHost(hostBuilder);
            }
            hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<HttpContextFactory>();
                services.AddTransient<IHttpContextFactory>((sp) =>
                {
                    var defaultContextFactory = sp.GetRequiredService<HttpContextFactory>();
                    var httpContextFactory = new WrappedHttpContextFactory(defaultContextFactory);
                    httpContextFactory.ConfigureContextFeature(contextFeatures =>
                    {
                        var authFeature = new HttpAuthenticationFeature() { User = testApp.User };
                        contextFeatures[typeof(IHttpAuthenticationFeature)] = authFeature;
                    });
                    return httpContextFactory;
                });
            });

            Startup.ConfigureHost(hostBuilder);

            hostBuilder.ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddProvider(testApp.LoggerProvider);
            });
            hostBuilder.UseContentRoot(WebProjectPath()).UseEnvironment(environmentName);

            testApp.Server = new TestServer(hostBuilder);
            testApp.ApplicationServices = testApp.Server.Host.Services;

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
         StubLoggerProvider LoggerProvider { get;  }
         IHostingEnvironment HostingEnvironment { get;  }

         IServiceProvider ApplicationServices { get;  }

         TestServer Server { get;  }
         ClaimsPrincipal User { get; set; }
    }

    public class WrappedHttpContextFactory : IHttpContextFactory
    {
        IHttpContextFactory _contextFactory;
        Action<IFeatureCollection> _configureFeatures;
        public WrappedHttpContextFactory(IHttpContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void ConfigureContextFeature(Action<IFeatureCollection> configureFeatures)
        {
            _configureFeatures = configureFeatures;
        }

        public HttpContext Create(IFeatureCollection contextFeatures)
        {
            if(_configureFeatures != null)
            {
                _configureFeatures(contextFeatures);
            }

            return _contextFactory.Create(contextFeatures);
        }

        public void Dispose(HttpContext httpContext)
        {
            _contextFactory.Dispose(httpContext);
        }
    }

    public class TestApplicationContext: IApplicationContext
    {
        public StubLoggerProvider LoggerProvider { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }        

        public IServiceProvider ApplicationServices { get; set; }

        public TestServer Server { get; set; }
        public ClaimsPrincipal User { get; set; }


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

            ApplicationServices = null;

            if (LoggerProvider != null)
            {
                LoggerProvider.LogItems.Clear();
                LoggerProvider = null;
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        #endregion
    }

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

    public static class TestApplicationContextExtensions
    {
        public static T CreateController<T>(this IApplicationContext app) where T : Controller
        {
            var services = app.ApplicationServices;

            var actionContext = new ActionContext(
                new DefaultHttpContext
                {
                    RequestServices = services,
                    User = app.User
                },
                new RouteData(),
                new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(T).GetTypeInfo()
                });

            var controllerFactory = services.GetService<IControllerFactory>();
            var controller = controllerFactory.CreateController(new ControllerContext(actionContext)) as T;

            return controller;
        }


        public static T GetService<T>(this IApplicationContext app) where T : class
        {
            return app.ApplicationServices.GetService<T>();
        }
        
        public static void MockUser(this Application app)
        {
            var userId = 1;
            var userName = "FancyUser";
            var lastSigninTime = DateTime.UtcNow.AddMinutes(-30);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString(), ClaimValueTypes.Integer32),
                new Claim(ClaimTypes.Name, userName, ClaimValueTypes.String),
                new Claim("SigninTime", lastSigninTime.Ticks.ToString(), ClaimValueTypes.Integer64)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            app.User = new DiscussionPrincipal(identity)
            {
                User = new User
                {
                    Id = userId,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    DisplayName = "Fancy User",
                    LastSeendAt = lastSigninTime,
                    UserName = userName
                }
            };
        }
        
        public static TController CreateControllerAndValidate<TController>(this IApplicationContext app, object model) where TController: Controller
        {
            var controller = app.CreateController<TController>();
            controller.TryValidateModel(model, null);
            return controller;
        }
        
        
        public static IEnumerable<StubLoggerProvider.LogItem> GetLogs(this IApplicationContext app)
        {
            var loggerProvider = app.ApplicationServices.GetRequiredService<ILoggerProvider>() as StubLoggerProvider;
            return loggerProvider?.LogItems;
        }
    }
}
