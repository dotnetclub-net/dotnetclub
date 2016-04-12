using Discussion.Web.Tests.Specs;
using System.Net;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("ServerSpecs")]
    public class NotFoundSpec
    {
        public const string NotFoundPath = "/something-not-defined";
        public const string NotFoundStaticFile = "/something-not-defined.css";


        private Server _server;
        public NotFoundSpec(Server server)
        {
            _server = server;
        }

        [Fact]
        public async void should_response_not_found_by_default()
        {
            // act
            var response = await _server.CreateRequest(NotFoundPath).GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
        }

        [Fact]
        public async void should_response_not_found_for_a_static_file_path()
        {
            // act
            var response = await _server.CreateRequest(NotFoundStaticFile).GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
        }

    }
}
