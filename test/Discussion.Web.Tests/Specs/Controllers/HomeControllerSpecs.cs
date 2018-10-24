using System.Net;
using Discussion.Tests.Common;
using Discussion.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
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

            var errorResult = homeController.Error();

            Assert.NotNull(errorResult);
            Assert.IsType<ViewResult>(errorResult);

            homeController.Response.StatusCode.ShouldEqual((int)HttpStatusCode.InternalServerError);
        }
    }
}
