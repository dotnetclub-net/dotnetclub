using Discussion.Tests.Common;
using Discussion.Web.Controllers;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class CommonControllerSpecs
    {
        private TestDiscussionWebApp _app;

        public CommonControllerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public void convert_markdown_to_html()
        {
            // Act
            var commonController = _app.CreateController<CommonController>();

            // Action
            dynamic htmlFromMd = commonController.RenderMarkdown("# Title");

            Assert.Equal("<h2>Title</h2>\n",  htmlFromMd.html);
        }

        
    }
}