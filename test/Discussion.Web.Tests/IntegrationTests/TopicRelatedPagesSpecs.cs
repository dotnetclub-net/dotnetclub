using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class TopicRelatedPagesSpecs
    {
        
        private readonly TestApplication _theApp;
        public TopicRelatedPagesSpecs(TestApplication theApp)
        {
            _theApp = theApp.Reset();
        }

        [Fact]
        public async Task should_serve_topic_list_page()
        {
            // arrange
            var request = _theApp.Server.CreateRequest("/topics");

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
            var request = _theApp.Server.CreateRequest("/topics/create");
            _theApp.MockUser();
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
            var request = _theApp.Server.CreateRequest("/topics/create");

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
            _theApp.MockUser();
            var request = _theApp.Server.CreateRequest("/topics")
                .WithFormContent(new Dictionary<string, string>
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
            var request = _theApp.Server.CreateRequest("/topics");
            _theApp.MockUser();

            // act
            var response = await request.PostAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task should_show_topic_detail_page()
        {
            // arrange
            _theApp.MockUser();
            var request = _theApp.Server.CreateRequest("/topics")
                .WithFormContent(new Dictionary<string, string>
                {
                    {"title", "中文字 &quot;title"},
                    {"content", "**some** <script>content</script>"},
                    {"type", "1"}
                });
            var createResponse = await request.PostAsync();
            var redirectToUrl = createResponse.Headers.Location.ToString();
            
            // act
            var requestDetail = _theApp.Server.CreateRequest(redirectToUrl);
            var response = await requestDetail.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            
            var content = response.ReadAllContent();
            content.ShouldContain("中文字 &amp;quot;title");
            content.ShouldNotContain("<br />title");
            
            content.ShouldContain("<strong>some</strong> &lt;script&gt;content&lt;/script&gt;");
            content.ShouldNotContain("<script>content</script>");
        }

    }
}
