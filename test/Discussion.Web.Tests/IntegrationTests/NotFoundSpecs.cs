using Discussion.Web.Tests.Specs;
using System.Net;
using System.Threading.Tasks;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class NotFoundSpecs
    {
        public const string NotFoundPath = "/something-not-defined";
        public const string NotFoundStaticFile = "/something-not-defined.css";

        private TestApplication _theApp;
        public NotFoundSpecs(TestApplication theApp)
        {
            _theApp = theApp;
        }

        [Fact]
        public async Task should_response_not_found_by_default()
        {
            // act
            var response = await _theApp.Server.CreateRequest(NotFoundPath).GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_response_not_found_for_a_static_file_path()
        {
            // act
            var response = await _theApp.Server.CreateRequest(NotFoundStaticFile).GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
        }

    }
}
