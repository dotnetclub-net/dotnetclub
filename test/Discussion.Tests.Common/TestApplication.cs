using System;
using System.Security.Claims;
using Discussion.Core;
using Discussion.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;

namespace Discussion.Tests.Common
{
    public class TestApplication: IDisposable
    {
        ClaimsPrincipal _originalUser;
        AntiForgeryRequestTokens _antiForgeryRequestTokens;

        protected void Init<TStartup>(Action<IWebHostBuilder> configureHost = null) where TStartup: class 
        {           
            BuildTestApplication<TStartup>(this, "UnitTest", configureHost);
            _originalUser = User;
        }

        protected void Reset()
        {
            _antiForgeryRequestTokens = null;
            User = _originalUser;
            ReplacableServiceProvider.Reset();
        }
        public IDataProtector dataProtector { get; private set; }

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
        
        public static TestApplication BuildTestApplication<TStartup>(TestApplication testApp, 
            string environmentName = "Production",
            Action<IWebHostBuilder> configureHost = null) where TStartup: class 
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

            var connectionStringEVKey = $"DOTNETCLUB_{ApplicationDataServices.ConfigKeyConnectionString}";
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(connectionStringEVKey)))
            {
                Environment.SetEnvironmentVariable(connectionStringEVKey, " ");
            }
            Environment.SetEnvironmentVariable("DOTNETCLUB_Logging:Console:LogLevel:Default", "Warning");
            
            Configuration.ConfigureHost(hostBuilder);
            hostBuilder.ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddProvider(testApp.LoggerProvider);
            });
            hostBuilder
                .UseContentRoot(TestEnv.WebProjectPath())
                .UseEnvironment(environmentName)
                .UseStartup<TStartup>();
            
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
}
