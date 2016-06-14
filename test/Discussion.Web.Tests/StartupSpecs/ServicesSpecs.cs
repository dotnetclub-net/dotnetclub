using Discussion.Web.Models;
using Discussion.Web.Data;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Microsoft.AspNetCore.Hosting;
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
            repo.GetType().GetGenericTypeDefinition().ShouldEqual(typeof(MongoRepository<,>));
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
            return CreateApplicationServices((s) => { });
        }

        public static IServiceProvider CreateApplicationServices(Action<IServiceCollection> configureServices) {
            const string dummyDB = "mongodb://localhost/dummydb";
            var services = new ServiceCollection();
            var startup = CreateMockStartup();
            startup.Configuration["mongoConnectionString"] = dummyDB;

            // services.AddInstance<IHostingEnvironment>(startup.HostingEnvironment);
            startup.ConfigureServices(services);

            services.AddScoped(typeof(IRepositoryContext), (serviceProvider) =>
            {
                return new MongoRepositoryContext(dummyDB);
            });
            configureServices(services);

            return services.BuildServiceProvider();
        }

        public static Startup CreateMockStartup()
        {
            var hostingEnv = new Mock<IHostingEnvironment>();
            hostingEnv.SetupGet(e => e.EnvironmentName).Returns("Development");
            hostingEnv.SetupGet(e => e.ContentRootPath).Returns(TestEnv.WebProjectPath());

            return new Startup(hostingEnv.Object);
        }

    }
}