using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Xunit;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class TopicRelatedPagesSpecs
    {
        
        private readonly TestDiscussionWebApp _app;
        public TopicRelatedPagesSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public async Task should_serve_topic_list_page()
        {
            // arrange
            var request = _app.Server.CreateRequest("/topics");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAllContent().ShouldContain("全部话题");
        }

        [Fact]
        public async Task should_serve_create_topic_page()
        {
            // arrange
            var request = _app.Server.CreateRequest("/topics/create");
            _app.MockUser();
            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAllContent().ShouldContain("创建新话题");
        }

        [Fact]
        public async Task should_redirect_to_signin_when_access_create_topic_page_without_user_principal()
        {
            // arrange
            var request = _app.Server.CreateRequest("/topics/create");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Contains("signin").ShouldEqual(true);
        }


        [Fact]
        public async Task should_accept_create_topic_request_with_valid_post_data()
        {
            // arrange
            _app.MockUser();
            var request = _app.RequestAntiForgeryForm("/topics",
                new Dictionary<string, string>
                {
                    {"title", "中文的 title"},
                    {"content", "some content"},
                    {"type", "1"}
                });

            // act
            var response = await request.PostAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.Redirect);
            response.Headers.Location.ShouldNotBeNull();
            response.Headers.Location.ToString().ShouldContain("/topics/", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task should_not_accept_create_topic_request_with_invalid_post_data()
        {
            // arrange
            _app.MockUser();
            var request = _app.RequestAntiForgeryForm("/topics");

            // act
            var response = await request.PostAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task should_show_topic_detail_page_with_markdown_content()
        {
            // arrange
            _app.MockUser();
            var request = _app.RequestAntiForgeryForm("/topics",
                new Dictionary<string, string>
                {
                    {"title", "中文字 &quot;title"},
                    {"content", "# This is a heading\n**some** <script>content</script>"},
                    {"type", "1"}
                });
            var createResponse = await request.PostAsync();
            var redirectToUrl = createResponse.Headers.Location.ToString();
            var postId = int.Parse(redirectToUrl.Substring(redirectToUrl.LastIndexOf('/') + 1));
            await ReplyPageSpecs.RequestToCreateReply(_app, postId, "# heading in reply\n*italic*");
                
            
            // act
            var requestDetail = _app.Server.CreateRequest(redirectToUrl);
            var response = await requestDetail.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            
            var content = response.ReadAllContent();
            content.ShouldContain("中文字 &amp;quot;title");
            content.ShouldNotContain("<br />title");
            
            content.ShouldContain("<h2>This is a heading</h2>\n\n<p><strong>some</strong> &lt;script&gt;content&lt;/script&gt;</p>");
            content.ShouldNotContain("<script>content</script>");
            
            content.ShouldContain("<h3>heading in reply</h3>\n\n<p><em>italic</em></p>");
        }

    }
}
