using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Net.Http;
using Xunit;

namespace Discussion.Web.Tests.Specs
{
    public sealed class Server : IDisposable
    {
        TestServer _testServerInstance;
        public TestServer TestServer {
            get
            {
                return _testServerInstance ?? (_testServerInstance = CreateServer());
            }
        }

        #region Proxy TestServer Methods

        public HttpClient CreateClient()
        {
            return this.TestServer.CreateClient();
        }

        public HttpMessageHandler CreateHandler()
        {
            return this.TestServer.CreateHandler();
        }

        public RequestBuilder CreateRequest(string path)
        {
            return this.TestServer.CreateRequest(path);
        }

        public WebSocketClient CreateWebSocketClient()
        {
            return this.TestServer.CreateWebSocketClient();
        }

        #endregion

        public static TestServer CreateServer(string environmentName = "Production")
        {
            return new TestServer(TestServer.CreateBuilder()
                 .UseServices(services =>
                 {
                     services.AddInstance<IApplicationEnvironment>(Application.CreateApplicationEnvironment());
                 })
                 .UseEnvironment(environmentName)
                 .UseStartup<Startup>());
        }


        #region Disposing

        ~Server()
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

            try
            {
               if(_testServerInstance != null)
                {
                    _testServerInstance.Dispose();
                }
            }
            catch { }
        }

        #endregion
    }

    // Use shared context to maintain database fixture
    // see https://xunit.github.io/docs/shared-context.html#collection-fixture
    [CollectionDefinition("ServerSpecs")]
    public class ServerCollection : ICollectionFixture<Server>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
