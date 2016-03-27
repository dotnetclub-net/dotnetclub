using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Startup;
using System.Collections.Generic;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNet.Builder;
using System;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Jusfr.Persistent;
using Discussion.Web.Models;
using Jusfr.Persistent.Mongo;
using Discussion.Web.Repositories;

namespace Discussion.Web.Tests.Startup
{
    public class MiddlewareConfigureSpecs
    {

        private RequestDelegate RequestHandler;
        private IServiceProvider ApplicationServices;

        public MiddlewareConfigureSpecs()
        {
            BuildRequestHandlerFromStartup();
        }


        [Fact]
        public void should_use_iis_platform()
        {
            // arrange
            var httpContext = GivenHttpContextFromIISPlatformHandler();

            // act
            RequestHandler.Invoke(httpContext);

            // assert
            AssertHttpContextAreProperlyRestored(httpContext);
        }

        [Fact]
        public void should_use_mvc()
        {
            var httpContext = CreateHttpContext();
            httpContext.Request.Path = RequestHandling.NotFoundSpec.NotFoundPath;

            RequestHandler.Invoke(httpContext);

            var loggerFactory = httpContext.ApplicationServices.GetRequiredService<ILoggerFactory>() as StubLoggerFactory;
            loggerFactory.ShouldNotBeNull();
            loggerFactory.LogItems.ShouldContain(item => item.Message.Equals("Request did not match any routes."));
        }

        [Fact]
        public void should_use_static_files()
        {
            var staticFile = RequestHandling.NotFoundSpec.NotFoundStaticFile;
            var httpContext = CreateHttpContext();
            httpContext.Request.Method = "GET";
            httpContext.Request.Path = staticFile;

            RequestHandler.Invoke(httpContext);

            var loggerFactory = httpContext.ApplicationServices.GetRequiredService<ILoggerFactory>() as StubLoggerFactory;
            loggerFactory.ShouldNotBeNull();
            loggerFactory.LogItems.ShouldContain(item => item.Message.Equals($"The request path {staticFile} does not match an existing file"));
        }

        private void BuildRequestHandlerFromStartup()
        {
            // arrange
            Func<IServiceCollection, IServiceProvider> configureServices;
            Action<IApplicationBuilder> configure;
            StartupMethods startup = null;
            IApplicationBuilder bootingupApp = null;

            configureServices = services =>
            {
                var loader = new StartupLoader(services.BuildServiceProvider(), new HostingEnvironment { EnvironmentName = "Production" });
                startup = loader.LoadMethods(typeof(Web.Startup), new List<string>());

                services.AddSingleton<ILoggerFactory, StubLoggerFactory>();
                startup.ConfigureServicesDelegate(services);

                ApplicationServices = services.BuildServiceProvider();
                return ApplicationServices;
            };
            configure = app =>
            {
                bootingupApp = app;
                startup.ConfigureDelegate(app);
            };
            
            new TestServer(TestServer.CreateBuilder().UseStartup(configure, configureServices));
            RequestHandler = bootingupApp.Build();
        }
        
        private DefaultHttpContext CreateHttpContext()
        {
            return new DefaultHttpContext
            {
                ApplicationServices = this.ApplicationServices
            };
        }

        const string Req_IP = "128.0.0.1";
        const string XForwardedForHeaderName = "X-Forwarded-For";

        const string Req_ProtoValue = "FTP";
        const string XForwardedProtoHeaderName = "X-Forwarded-Proto";

        const string Req_ProtoAfterModified = "FTP-OVER-HTTP";        
        const string XOriginalProtoName = "X-Original-Proto";


        HttpContext GivenHttpContextFromIISPlatformHandler()
        {
            // IISPlatformHandlerMiddleware is used to restore the Headers marked(modified) by the IISPlatformHandler module
            // see https://github.com/aspnet/IISIntegration/blob/2fe2e0d8418ff55612fc9001b7f7bde058ae5bb9/src/Microsoft.AspNet.IISPlatformHandler/IISPlatformHandlerMiddleware.cs

            var httpContext = CreateHttpContext();
            httpContext.Request.Headers.Add(XForwardedProtoHeaderName, Req_ProtoValue);
            httpContext.Request.Headers.Add(XForwardedForHeaderName, Req_IP);

            httpContext.Request.Scheme = Req_ProtoAfterModified;

            return httpContext;
        }

        void AssertHttpContextAreProperlyRestored(HttpContext httpContext)
        {
            httpContext.Request.Scheme.ShouldEqual(Req_ProtoValue);
            // httpContext.Request.Headers.ShouldContain(kvp => kvp.Key == XOriginalProtoName && kvp.Value == Req_ProtoAfterModified);

            httpContext.Connection.RemoteIpAddress.ToString().ShouldEqual(Req_IP);
        }
    }

    class StubLoggerFactory : ILoggerFactory
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
            throw new NotImplementedException();
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
                throw new NotImplementedException();
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
}
