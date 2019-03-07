﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Resources;
using Discussion.Web.Services.UserManagement.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Discussion.Web.Tests.StartupSpecs
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
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("IApiDescriptionProvider"));
            services.ShouldContain(x => x.ServiceType.ToString().EndsWith("MvcMarkerService"));
        }

        [Fact]
        public void should_use_ef_repository()
        {
            // arrange
            var applicationServices = CreateApplicationServices();

            // act
            var repo = applicationServices.GetRequiredService<IRepository<Article>>();

            // assert
            repo.ShouldNotBeNull();
            repo.GetType().GetGenericTypeDefinition().ShouldEqual(typeof(EfRepository<>));
        }
        
        [Fact]
        public void should_add_identity_services()
        {
            var applicationServices = CreateApplicationServices();

            var userManager = applicationServices.GetRequiredService<UserManager<User>>();
            var errorDescriber = applicationServices.GetRequiredService<IdentityErrorDescriber>();

            Assert.IsType<EmailAddressAwareUserManager<User>>(userManager);
            Assert.IsType<ResourceBasedIdentityErrorDescriber>(errorDescriber);
        }
        
        [Fact]
        public void should_use_translated_model_binding_messages()
        {
            var applicationServices = CreateApplicationServices();

            var options = applicationServices.GetRequiredService<IOptions<MvcOptions>>().Value;

            var methodType = options.ModelBindingMessageProvider.ValueIsInvalidAccessor.Method.DeclaringType;
            var containingType = methodType.ReflectedType;
            Assert.Equal(typeof(ResourceModelBindingMessageExtensions), containingType);
        }

        static IServiceProvider CreateApplicationServices()
        {
            return CreateApplicationServices(c => { },  s => { });
        }

        static IServiceProvider CreateApplicationServices(Action<Mock<IConfiguration>> configureSettings, Action<IServiceCollection> configureServices) {
            var services = new ServiceCollection();
            var startup = CreateMockStartup(configureSettings);
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            startup.ConfigureServices(services);
            configureServices(services);

            return services.BuildServiceProvider();
        }

        private static Startup CreateMockStartup(Action<Mock<IConfiguration>> configureSettings)
        {
            var hostingEnv = new Mock<IHostingEnvironment>();
            hostingEnv.SetupGet(e => e.EnvironmentName).Returns("UnitTest");
            hostingEnv.SetupGet(e => e.ContentRootPath).Returns(TestEnv.WebProjectPath());

            var appConfig = new Mock<IConfiguration>();
            appConfig.SetupGet(e => e[It.IsAny<string>()]).Returns((string)null);
            configureSettings(appConfig);

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(TypeNameHelper.GetTypeDisplayName(typeof(Startup)))).Returns(NullLogger.Instance);
                
            return new Startup(hostingEnv.Object, appConfig.Object, loggerFactory.Object);
        }

    }
}