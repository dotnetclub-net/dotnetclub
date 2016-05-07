using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Builder;
using System;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Extensions.Logging;
using Discussion.Web.Tests.Specs;

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
            httpContext.Request.Path = IntegrationTests.NotFoundSpecs.NotFoundPath;

            RequestHandler.Invoke(httpContext);

            var loggerFactory = httpContext.ApplicationServices.GetRequiredService<ILoggerFactory>() as StubLoggerFactory;
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

            var loggerFactory = httpContext.ApplicationServices.GetRequiredService<ILoggerFactory>() as StubLoggerFactory;
            loggerFactory.ShouldNotBeNull();
            loggerFactory.LogItems.ShouldContain(item => item.Message.Equals($"The request path {staticFile} does not match an existing file"));
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

}
