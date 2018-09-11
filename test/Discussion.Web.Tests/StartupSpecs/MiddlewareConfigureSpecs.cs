using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace Discussion.Web.Tests.StartupSpecs
{
    [Collection("AppSpecs")]
    public class MiddlewareConfigureSpecs
    {

        private TestServer server;
        public MiddlewareConfigureSpecs(Application app)
        {
            server = app.Server;
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
        public async Task should_use_mvc()
        {
            HttpContext httpContext = null;
            await server.SendAsync(ctx =>
            {
                httpContext = ctx;
                ctx.Request.Path = IntegrationTests.NotFoundSpecs.NotFoundPath;
            });
            

            var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>() as StubLoggerFactory;
            loggerFactory.ShouldNotBeNull();
            loggerFactory.LogItems.ShouldContain(item => item.Message.Equals("Request did not match any routes."));
        }

        [Fact]
        public async Task should_use_static_files()
        {
            var staticFile = IntegrationTests.NotFoundSpecs.NotFoundStaticFile;
            HttpContext httpContext = null;
            
            await server.SendAsync(ctx =>
            {
                httpContext = ctx;
//                ctx.Features.Set<IHttpResponseFeature>(new DummyHttpResponseFeature());
                ctx.Request.Method = "GET";
                ctx.Request.Path = staticFile;
            });

            var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>() as StubLoggerFactory;
            loggerFactory.ShouldNotBeNull();
            loggerFactory.LogItems.ShouldContain(item => item.Message.Equals($"The request path {staticFile} does not match an existing file"));
        }
    }

}
