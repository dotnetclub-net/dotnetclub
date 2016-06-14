using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace Discussion.Web.Tests.StartupSpecs
{
    [Collection("AppSpecs")]
    public class MiddlewareConfigureSpecs
    {

        private RequestDelegate RequestHandler;
        private IServiceProvider ApplicationServices;
        public MiddlewareConfigureSpecs(Application app)
        {
            RequestHandler = app.RequestHandler;
            ApplicationServices = app.ApplicationServices;
        }


        [Fact]
        public void should_use_iis_platform()
        {
            var app = Application.BuildApplication("Dev", host =>
            {
                host.UseSetting("PORT", "5000");
                host.UseSetting("APPL_PATH", "/");
                host.UseSetting("TOKEN", "dummy-token");
            });

            var iisFilter = app.ApplicationServices.GetRequiredService<IStartupFilter>();

            var filterName = iisFilter.GetType().FullName;
            filterName.Contains("IISSetupFilter").ShouldEqual(true);

            (app as IDisposable).Dispose();
        }

        [Fact]
        public void should_use_mvc()
        {
            var httpContext = CreateHttpContext();
            httpContext.Request.Path = IntegrationTests.NotFoundSpecs.NotFoundPath;

            RequestHandler.Invoke(httpContext);

            var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>() as StubLoggerFactory;
            loggerFactory.ShouldNotBeNull();
            loggerFactory.LogItems.ShouldContain(item => item.Message.Equals("Request did not match any routes."));
        }

        [Fact]
        public void should_use_static_files()
        {
            var staticFile = IntegrationTests.NotFoundSpecs.NotFoundStaticFile;
            var httpContext = CreateHttpContext();
            httpContext.Request.Method = "GET";
            httpContext.Request.Path = staticFile;

            RequestHandler.Invoke(httpContext);

            var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>() as StubLoggerFactory;
            loggerFactory.ShouldNotBeNull();
            loggerFactory.LogItems.ShouldContain(item => item.Message.Equals($"The request path {staticFile} does not match an existing file"));
        }
        
        
        private DefaultHttpContext CreateHttpContext()
        {
            return new DefaultHttpContext
            {
                RequestServices = this.ApplicationServices
            };
        }

    }

}
