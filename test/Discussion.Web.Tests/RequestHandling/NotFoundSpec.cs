using Microsoft.AspNet.TestHost;
using System.Net;
using Xunit;


namespace Discussion.Web.Tests.RequestHandling
{
    public class NotFoundSpec
    {
        public const string NotFoundPath = "/something-not-defined";

        [Fact]
        public async void should_response_not_found_by_default()
        {
            // arrange
            var server = new TestServer(TestServer.CreateBuilder()
                   .UseStartup<Web.Startup>()
                   .UseEnvironment("Production"));

            // act
            var response = await server.CreateRequest(NotFoundPath).GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
        }
    }
}
