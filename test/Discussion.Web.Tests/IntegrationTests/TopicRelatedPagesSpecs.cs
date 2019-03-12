using System;
using System.Net;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Xunit;
using static Discussion.Tests.Common.SigninRequirement;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class TopicRelatedPagesSpecs
    {

        private readonly TestDiscussionWebApp _app;
        public TopicRelatedPagesSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public void should_serve_topic_list_page()
        {
            _app.ShouldGet("/topics",
                SigninNotRequired,
                responseShouldContain: "全部话题");
        }

        [Fact]
        public void should_serve_create_topic_page()
        {
            _app.ShouldGet("/topics/create",
                SigninRequired,
                responseShouldContain: "创建新话题");
        }

        [Fact]
        public void should_accept_create_topic_request_with_valid_post_data()
        {
            _app.ShouldPost("/topics",
                new
                {
                    title = "中文的 title",
                    content = "some content",
                    type = "1"
                },
                SigninRequired)
                .WithResponse(res =>
                {
                    res.StatusCode.ShouldEqual(HttpStatusCode.Redirect);
                    res.Headers.Location.ShouldNotBeNull();
                    res.Headers.Location.ToString().ShouldContain("/topics/", StringComparison.OrdinalIgnoreCase);
                });
        }

        [Fact]
        public void should_not_accept_create_topic_request_with_invalid_post_data()
        {
            _app.Path("/topics")
                .Post()
                .ShouldFail(_app.MockUser())
                .WithResponse(res =>
                {
                    res.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                });
        }

        [Fact]
        public void should_show_topic_detail_page_with_markdown_content()
        {
            string topicUrl = null;

            _app.Path("/topics")
                .Post()
                .WithForm(new
                {
                    title = "中文字 &quot;title",
                    content = "# This is a heading\n**some** <script>content</script>",
                    type = "1"
                })
                .ShouldSuccessWithRedirect(_app.MockUser())
                .WithResponse(res =>
                {
                    topicUrl = res.Headers.Location.ToString();
                });

            var topicId = int.Parse(topicUrl.Substring(topicUrl.LastIndexOf('/') + 1));
            _app.Path($"/topics/{topicId}/replies")
                .Post()
                .WithForm(new
                {
                    Content = "# heading in reply\n*italic*"
                })
                .ShouldSuccess(_app.MockUser());

            _app.ShouldGet(topicUrl, SigninNotRequired)
                .WithResponse(response =>
                {
                    response.StatusCode.ShouldEqual(HttpStatusCode.OK);

                    var content = response.ReadAllContent();
                    content.ShouldContain("中文字 &amp;quot;title");
                    content.ShouldNotContain("<br />title");

                    content.ShouldContain("<h2>This is a heading</h2>\n\n<p><strong>some</strong> &lt;script&gt;content&lt;/script&gt;</p>");
                    content.ShouldNotContain("<script>content</script>");

                    content.ShouldContain("<h3>heading in reply</h3>\n\n<p><em>italic</em></p>");
                });
        }
    }
}
