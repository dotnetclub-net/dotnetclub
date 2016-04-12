using Discussion.Web.Controllers;
using Microsoft.AspNet.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Web
{

    [Collection("AppSpecs")]
    public class TopicControllerSpecs
    {
        public Application _myApp;
        public TopicControllerSpecs(Application app)
        {
            _myApp = app;
        }


        [Fact]
        public void should_serve_about_page_as_view_result()
        {
            var topicController = _myApp.ApplicationServices.CreateController<TopicController>();

            var topicListResult = topicController.List();

            Assert.NotNull(topicListResult);
            Assert.IsType<ViewResult>(topicListResult);
        }




    }
}
