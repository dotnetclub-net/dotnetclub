using Xunit;
using Discussion.Web.Controllers;
using Microsoft.AspNet.Mvc;
using System.Net;

namespace Discussion.Web.Tests.Specs.Web
{
    [Collection("AppSpecs")]
    public class HomeControllerSpecs
    {
        public Application _myApp;
        public HomeControllerSpecs(Application app)
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
