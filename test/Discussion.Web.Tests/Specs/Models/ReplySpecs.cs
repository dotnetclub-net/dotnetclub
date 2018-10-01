using Discussion.Web.Models;
using Xunit;

namespace Discussion.Web.Tests.Specs.Models
{
    public class ReplySpecs
    {
        [Fact]
        public void should_get_markdown_content_with_max_heading_level_3()
        {
            var reply = new Reply
            {
                Content = @"Some **bold** content
## Heading 2"
            };

            var htmlContent = reply.GetContentAsHtml();

            var expectedRenderedHtml = "<p>Some <strong>bold</strong> content</p>\n<h3>Heading 2</h3>\n";
            Assert.Equal(expectedRenderedHtml, htmlContent);
        }
    }
}