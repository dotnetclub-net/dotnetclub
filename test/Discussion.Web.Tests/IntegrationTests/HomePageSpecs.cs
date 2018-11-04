using System.Net;
using System.Threading.Tasks;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class HomePageSpecs
    {
        private TestDiscussionWebApp _app;
        public HomePageSpecs(TestDiscussionWebApp app) {
            _app = app;
        }


        public const string HomePagePath = "/";
        public const string ErrorPagePath = "/error";

        [Fact]
        public async Task should_serve_home_page_correctly()
        {
            // arrange
            var request = _app.Server.CreateRequest(HomePagePath);

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAllContent().ShouldContain("全部话题");            
        }


        [Fact]
        public async Task should_serve_about_page()
        {
            // arrange
            var request = _app.Server.CreateRequest("/about");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAllContent().ShouldContain("本站简介");
        }



        [Fact]
        public async Task should_serve_error_as_error_response()
        {
            // arrange
            var request = _app.Server.CreateRequest(ErrorPagePath);

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            response.ReadAllContent().ShouldContain("An error occured");
        }

    }
}
