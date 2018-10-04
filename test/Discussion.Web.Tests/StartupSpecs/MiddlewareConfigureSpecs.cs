using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace Discussion.Web.Tests.StartupSpecs
{
    [Collection("AppSpecs")]
    public class MiddlewareConfigureSpecs
    {

        private readonly TestServer server;
        private readonly TestApplication _app;
        public MiddlewareConfigureSpecs(TestApplication app)
        {
            this._app = app;
            server = app.Server;
        }


        [Fact]
        public void should_use_iis_platform()
        {
            var app = TestApplication.BuildApplication(new TestApplication(initlizeApp: false), "UnitTest", host =>
            {
                host.UseSetting("PORT", "5000");
                host.UseSetting("APPL_PATH", "/");
                host.UseSetting("TOKEN", "dummy-token");
            });

            var filters = app.Server.Host
                .Services
                .GetServices<IStartupFilter>()
                .ToList();

            filters.ShouldContain(f => f.GetType().FullName.Contains("IISSetupFilter"));

            (app as IDisposable).Dispose();
        }

        [Fact]
        public async Task should_use_mvc()
        {
            HttpContext httpContext = null;
            await server.SendAsync(ctx =>
            {
                httpContext = ctx;
                ctx.Request.Path = IntegrationTests.HomePageSpecs.HomePagePath;
            });
            

            var logs = _app.GetLogs();
            logs.ShouldNotBeNull();
            logs.ShouldContain(item => item.Category.StartsWith("Microsoft.AspNetCore.Mvc"));
        }

        [Fact]
        public async Task should_use_static_files()
        {
            var staticFile = IntegrationTests.NotFoundSpecs.NotFoundStaticFile;
            HttpContext httpContext = null;
            
            await server.SendAsync(ctx =>
            {
                httpContext = ctx;
                ctx.Request.Method = "GET";
                ctx.Request.Path = staticFile;
            });

            var logs = _app.GetLogs();
            logs.ShouldNotBeNull();
            logs.ShouldContain(item => item.Category.StartsWith("Microsoft.AspNetCore.StaticFiles"));
        }
        
        
        [Fact]
        public void should_use_temporary_database_when_no_database_connection_string_specified()
        {
            var app = TestApplication.BuildApplication(new TestApplication(initlizeApp: false), "UnitTest");

            var logs = app.GetLogs();
            
            logs.ShouldNotBeNull();
            logs.ShouldContain(item => item.Message.Contains("数据库结构创建完成"));
            
            (app as IDisposable).Dispose();
        }
    }

}
