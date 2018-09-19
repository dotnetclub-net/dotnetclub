using System.Net;
using System.Threading.Tasks;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class HomePageSpecs
    {
        private TestApplication _theApp;
        public HomePageSpecs(TestApplication theApp) {
            _theApp = theApp;
        }


        public const string HomePagePath = "/";
        public const string ErrorPagePath = "/error";

        [Fact]
        public async Task should_serve_home_page_correctly()
        {
            // arrange
            var request = _theApp.Server.CreateRequest(HomePagePath);

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Content().ShouldContain("全部话题");            
        }


        [Fact]
        public async Task should_serve_about_page()
        {
            // arrange
            var request = _theApp.Server.CreateRequest("/about");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Content().ShouldContain("本站简介");
        }



        [Fact]
        public async Task should_serve_error_as_error_response()
        {
            // arrange
            var request = _theApp.Server.CreateRequest(ErrorPagePath);

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            response.Content().ShouldContain("An error occured");
        }

    }
}
