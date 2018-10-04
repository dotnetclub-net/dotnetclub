using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xunit;
using static Discussion.Web.Tests.TestEnv;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Security.Claims;
using Discussion.Core;
using Microsoft.AspNetCore.Hosting.Internal;

namespace Discussion.Web.Tests
{
    public sealed class TestApplication : IDisposable
    {
        readonly ClaimsPrincipal _originalUser;
        AntiForgeryRequestTokens _antiForgeryRequestTokens;


        public TestApplication() : this(true){ }

        internal TestApplication(bool initlizeApp)
        {
            if (initlizeApp)
            {
                BuildApplication(this, "UnitTest");
                // BuildApplication(this);
            }

            _originalUser = User;
        }


        public TestApplication Reset()
        {
            _antiForgeryRequestTokens = null;
            User = _originalUser;
            ReplacableServiceProvider.Reset();
            return this;
        }


        public StubLoggerProvider LoggerProvider { get; private set; }
        public IHostingEnvironment HostingEnvironment { get; private set; }

        public IServiceProvider ApplicationServices {get; private set; }
        public ReplacableServiceProvider ServiceReplacer { get; private set; }

        public TestServer Server {get; private set;  }
        public ClaimsPrincipal User{ get; set;}

        public AntiForgeryRequestTokens GetAntiForgeryTokens()
        {
            if (_antiForgeryRequestTokens == null)
            {
                _antiForgeryRequestTokens = AntiForgeryRequestTokens.GetFromApplication(this);
            }
            
            return _antiForgeryRequestTokens;
        }
        
        public static TestApplication BuildApplication(TestApplication testApp, string environmentName = "Production", Action<IWebHostBuilder> configureHost = null)
        {
            testApp.LoggerProvider = new StubLoggerProvider();
            testApp.User = new ClaimsPrincipal(new ClaimsIdentity());
            
            var hostBuilder = new WebHostBuilder();
            configureHost?.Invoke(hostBuilder);
            hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<HttpContextFactory>();
                services.AddTransient<IHttpContextFactory>((sp) =>
                {
                    var defaultContextFactory = sp.GetService<HttpContextFactory>();
                    var httpContextFactory = new WrappedHttpContextFactory(defaultContextFactory);
                    httpContextFactory.ConfigureFeatureWithContext((features, httpCtx) =>
                    {
                        features.Set<IHttpAuthenticationFeature>(new HttpAuthenticationFeature { User = testApp.User });
                        features.Set<IServiceProvidersFeature>(new RequestServicesFeature(httpCtx, testApp.ServiceReplacer.CreateScopeFactory()));
                    });
                    return httpContextFactory;
                });
            });

            Environment.SetEnvironmentVariable("DOTNETCLUB_sqliteConnectionString", " ");
            Environment.SetEnvironmentVariable("DOTNETCLUB_Logging:Console:LogLevel:Default", "Warning");
            Configuration.ConfigureHost(hostBuilder);

            hostBuilder.ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddProvider(testApp.LoggerProvider);
            });
            hostBuilder
                .UseContentRoot(WebProjectPath())
                .UseEnvironment(environmentName)
                .UseStartup<Startup>();
            
            testApp.Server = new TestServer(hostBuilder);
            testApp.ApplicationServices = testApp.Server.Host.Services;
            testApp.ServiceReplacer = new ReplacableServiceProvider(testApp.ApplicationServices);
            
            return testApp;
        }
        
        #region Disposing

        ~TestApplication()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            Reset();
            
            ApplicationServices = null;

            if (LoggerProvider != null)
            {
                LoggerProvider.LogItems.Clear();
                LoggerProvider = null;
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        #endregion
    }

    // Use shared context to maintain database fixture
    // see https://xunit.github.io/docs/shared-context.html#collection-fixture
    [CollectionDefinition("AppSpecs")]
    public class ApplicationCollection : ICollectionFixture<TestApplication>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

}
