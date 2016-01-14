using Microsoft.AspNet.Builder.Internal;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.RequestHandling
{
    public class NotFoundSpec
    {
        [Fact]
        public async void should_response_not_found_by_default()
        {
            // arrage
            var services = new ServiceCollection();
            services.AddLogging();
            var startup = new Web.Startup();
            startup.ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();
            var app = new ApplicationBuilder(serviceProvider);
            startup.Configure(app, (new Mock<IHostingEnvironment>()).Object);

            // act
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/something-not-defined";

            // assert
            httpContext.Response.StatusCode.ShouldEqual(200);



            // act
            await app.Build().Invoke(httpContext);

            // assert
            httpContext.Response.StatusCode.ShouldEqual(404);
            // httpContext.Response.Body
        }
    }
}
