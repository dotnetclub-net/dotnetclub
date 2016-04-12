using Discussion.Web.Tests.Specs;
using System.Net;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class HomePageSpec
    {
        private Application _theApp;
        public HomePageSpec(Application theApp) {
            _theApp = theApp;
        }


        public const string HomePagePath = "/";
        public const string ErrorPagePath = "/error";

        [Fact]
        public async void should_handle_home_page_correctly()
        {
            // arrange
            var request = _theApp.Server.CreateRequest(HomePagePath);

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }


        [Fact]
        public async void should_serve_about_page()
        {
            // arrange
            var request = _theApp.Server.CreateRequest("/about");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }



        [Fact]
        public async void should_serve_error_as_error_response()
        {
            // arrange
            var request = _theApp.Server.CreateRequest(ErrorPagePath);

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
        }

    }
}
