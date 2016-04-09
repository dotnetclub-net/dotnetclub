using Discussion.Web.Tests.Specs;
using System.Net;
using Xunit;


namespace Discussion.Web.Tests.RequestHandling
{
    [Collection("ServerSpecs")]
    public class HomePageSpec
    {
        private Server _server;
        public HomePageSpec(Server server) {
            _server = server;
        }



        public const string HomePagePath = "/";
        public const string ErrorPagePath = "/error";

        [Fact]
        public async void should_handle_home_page_correctly()
        {
            // arrange
            var request = _server.CreateRequest(HomePagePath);

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }



        [Fact]
        public async void should_serve_error_as_error_response()
        {
            // arrange
            var request = _server.CreateRequest(ErrorPagePath);

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
        }

    }
}
