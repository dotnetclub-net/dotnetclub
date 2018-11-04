using Discussion.Admin.Controllers;
using Discussion.Tests.Common;
using Xunit;

namespace Discussion.Admin.Tests.Specs.Controllers
{
    [Collection("AdminSpecs")]
    public class HomeControllerSpecs
    {
        private TestDiscussionAdminApp _app;

        public HomeControllerSpecs(TestDiscussionAdminApp app)
        {
            _app = app;
        }

        [Fact]
        public void should_handle_index_action()
        {
            var homeController = _app.CreateController<HomeController>();
            
            var indexResult = homeController.Index();

            Assert.Equal("Hello Admin", indexResult.Content);

        }
    }
}