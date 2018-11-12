using System;
using System.Collections.Generic;
using System.Linq;
using Discussion.Admin.Supporting;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;


namespace Discussion.Admin.Tests.StartupSpecs
{
    
    public class ServicesSpecs
    {
        [Fact]
        public void should_add_mvc_services()
        {
            // arrage
            IServiceCollection services = null;

            // act
            CreateApplicationServices(c => { }, serviceCollection => {
                services = serviceCollection;
            });

            // assert
            services.Count.ShouldGreaterThan(0);
            services.ShouldNotEmpty();
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IControllerActivator"));
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IControllerFactory"));
        }
        
        
        [Fact]
        public void should_add_jwt_bearer_services()
        { // arrange
            var applicationServices = CreateApplicationServices();

            // act
            var jwtHandler = applicationServices.GetRequiredService<JwtBearerHandler>();

            // assert
            jwtHandler.ShouldNotBeNull();
        }
          
        [Fact]
        public void should_add_spa_services()
        { // arrange
            var applicationServices = CreateApplicationServices();

            // act
            var spaStaticFileProvider = applicationServices.GetRequiredService<ISpaStaticFileProvider>();

            // assert
            spaStaticFileProvider.ShouldNotBeNull();
        }
        
        
        
        
        static IServiceProvider CreateApplicationServices()
        {
            return CreateApplicationServices(c => { },  s => { });
        }

        static IServiceProvider CreateApplicationServices(Action<Mock<IConfiguration>> configureSettings, Action<IServiceCollection> configureServices) {
            var services = new ServiceCollection();
            var startup = CreateMockStartup(configureSettings, services);
            startup.ConfigureServices(services);
            configureServices(services);

            return services.BuildServiceProvider();
        }

        private static Startup CreateMockStartup(Action<Mock<IConfiguration>> configureSettings, ServiceCollection services)
        {
            var hostingEnv = new Mock<IHostingEnvironment>();
            hostingEnv.SetupGet(e => e.EnvironmentName).Returns("UnitTest");
            hostingEnv.SetupGet(e => e.ContentRootPath).Returns(TestEnv.WebProjectPath());

            var appConfig = new Mock<IConfiguration>();
            appConfig.Setup(e => e.GetSection(nameof(JwtIssuerOptions))).Returns(ComposeDummyJwtConfiguration());
            appConfig.SetupGet(e => e[It.IsAny<string>()]).Returns((string)null);
            configureSettings(appConfig);

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(NullLogger.Instance);
                
            services.AddSingleton(loggerFactory.Object);
            services.AddSingleton(hostingEnv.Object);
            return new Startup(hostingEnv.Object, appConfig.Object, loggerFactory.Object);
        }

        private static ConfigurationSection ComposeDummyJwtConfiguration()
        {
            var jwtOptions = new Dictionary<string, string>()
            {
                {"Secret", Guid.NewGuid().ToString()},
                {nameof(JwtIssuerOptions.Issuer), "admin"},
                {nameof(JwtIssuerOptions.Audience), "you"}
            };

            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(jwtOptions);
            return new ConfigurationSection(new ConfigurationRoot(builder.Build().Providers.ToList()), "");
        }
    }
}