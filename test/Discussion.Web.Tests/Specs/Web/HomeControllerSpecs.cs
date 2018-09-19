using Xunit;
using Discussion.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Discussion.Web.Tests.Specs.Web
{
    [Collection("AppSpecs")]
    public class HomeControllerSpecs
    {
        private readonly TestApplication _myApp;
        public HomeControllerSpecs(TestApplication app)
        {
            _myApp = app;
        }



        [Fact]
        public void should_serve_about_page_as_view_result()
        {
            var homeController = new HomeController();

            var aboutResult = homeController.About();

            Assert.NotNull(aboutResult);
            Assert.IsType<ViewResult>(aboutResult);
        }


        [Fact]
        public void should_serve_error_page_as_view_result()
        {
            var homeController = _myApp.CreateController<HomeController>();

            var errorResult = homeController.Error().Result;

            Assert.NotNull(errorResult);
            Assert.IsType<ViewResult>(errorResult);

            homeController.Response.StatusCode.ShouldEqual((int)HttpStatusCode.InternalServerError);
        }
    }
}
