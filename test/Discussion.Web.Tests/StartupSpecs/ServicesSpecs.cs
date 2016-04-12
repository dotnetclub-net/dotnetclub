using Discussion.Web.Models;
using Discussion.Web.Repositories;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Discussion.Web.Tests.StartupSpecs
{
    public class ServicesSpecs
    {
        [Fact]
        public void should_add_mvc_service()
        {
            // arrage
            IServiceCollection services = null;

            // act
            CreateApplicationServices(serviceCollection => {
                services = serviceCollection;
            });

            // assert
            services.Count.ShouldGreaterThan(0);
            services.ShouldNotEmpty();
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IControllerActivator"));
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IControllerFactory"));
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IApiDescriptionProvider"));
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("MvcMarkerService"));
        }


        [Fact]
        public void should_add_mongo_reposotiry()
        {
            // arrange
            var applicationServices = CreateApplicationServices();

            // act
            var repo = applicationServices.GetRequiredService<Repository<Article, int>>();

            // assert
            repo.ShouldNotBeNull();
            repo.GetType().GetGenericTypeDefinition().ShouldEqual(typeof(MongoRepository<, >));
        }


        [Fact]
        public void should_add_base_implemention_for_repository()
        {
            // arrage
            var applicationServices = CreateApplicationServices();

            // act
            var repo = applicationServices.GetRequiredService<IDataRepository<Article>>();

            // assert
            repo.ShouldNotBeNull();
            repo.GetType().ShouldEqual(typeof(BaseDataRepository<Article>));
        }

        public static IServiceProvider CreateApplicationServices()
        {
            return CreateApplicationServices((s)=> { });
        }

        public static IServiceProvider CreateApplicationServices(Action<IServiceCollection> configureServices) {
            var services = new ServiceCollection();
            var startup = CreateMockStartup();

            services.AddInstance<IApplicationEnvironment>(startup.ApplicationEnvironment);
            services.AddInstance<IHostingEnvironment>(startup.HostingEnvironment);
            startup.ConfigureServices(services);

            services.AddScoped(typeof(IRepositoryContext), (serviceProvider) =>
            {
                return new MongoRepositoryContext("mongodb://localhost/dummydb");
            });
            configureServices(services);

            return services.BuildServiceProvider();
        }

        public static Web.Startup CreateMockStartup()
        {
            var appEnv = new Mock<IApplicationEnvironment>();
            appEnv.SetupGet(e => e.ApplicationBasePath)
                .Returns(Directory.GetCurrentDirectory());

            var hostingEnv = new Mock<IHostingEnvironment>();
            hostingEnv.SetupGet(e => e.EnvironmentName)
                .Returns("Development");

            return new Web.Startup(hostingEnv.Object, appEnv.Object);
        }

    }
}