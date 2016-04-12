using Xunit;
using Discussion.Web.Controllers;
using Microsoft.AspNet.Mvc;
using System.Net;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Routing;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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
            var services = _myApp.ApplicationServices;
            var controllerFactory = services.GetService<IControllerFactory>();


            var controllerType = typeof(HomeController);
            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = controllerType.GetTypeInfo()
            };
            var httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };
            var context = new ActionContext(httpContext, new RouteData(), actionDescriptor);

            var homeController = controllerFactory.CreateController(context) as HomeController;


            var errorResult = homeController.Error();

            Assert.NotNull(errorResult);
            Assert.IsType<ViewResult>(errorResult);

            homeController.Response.StatusCode.ShouldEqual((int)HttpStatusCode.InternalServerError);
        }
    }
}
