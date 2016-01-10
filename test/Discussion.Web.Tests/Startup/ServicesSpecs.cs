using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Discussion.Web.Tests.Startup
{
    public class ServicesSpecs
    {
        [Fact]
        public void should_add_mvc_service()
        {
            // arrage
            var services = new ServiceCollection();
            var startup = new Web.Startup();

            // act
            startup.ConfigureServices(services);

            // assert
            services.Count.ShouldGreaterThan(0);
            services.ShouldNotEmpty();
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IControllerActivator"));
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IControllerFactory"));
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IApiDescriptionProvider"));
        }
    }
}
