using System.Net;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class HomePageSpecs
    {
        private readonly TestDiscussionWebApp _app;
        public HomePageSpecs(TestDiscussionWebApp app) {
            _app = app;
        }


        public const string ErrorPagePath = "/error";

        [Fact]
        public void should_serve_home_page_correctly()
        {
            _app.ShouldGet("/", responseShouldContain: "全部话题");
        }

        [Fact]
        public void should_serve_about_page()
        {
            _app.ShouldGet("/about", responseShouldContain: "本站简介");
        }

        [Fact]
        public void should_serve_error_as_error_response()
        {
            _app.Path("/error")
                .Get()
                .ShouldFail()
                .WithResponse(res =>
                {
                    res.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
                    res.ReadAllContent().ShouldContain("An error occured");
                });
        }

    }
}
