using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Mvc;
using Discussion.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class CommonRelatedApiSpecs
    {
        private TestDiscussionWebApp _app;
        public CommonRelatedApiSpecs(TestDiscussionWebApp app) {
            _app = app;
        }

        [Fact]
        public async Task should_serve_convert_md2html_api_correctly()
        {
            _app.MockUser();
            // arrange
            var request = _app.RequestAntiForgeryForm("/api/common/md2html",
                new Dictionary<string, string>
                {
                    {"markdown", "# 中文的 title"}
                });

            // act
            var response = await request.PostAsync();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            var jsonObj = JsonConvert.DeserializeObject<ApiResponse>(response.ReadAllContent());
            var result = jsonObj.Result as JObject;
            Assert.Equal("<h2>中文的 title</h2>\n", result["html"]);
        }
        
        [Fact]
        public async Task should_block_non_authorized_user()
        {
            // arrange
            var request = _app.RequestAntiForgeryForm("/api/common/md2html",
                new Dictionary<string, string>
                {
                    {"markdown", "# 中文的 title"}
                });

            // act
            var response = await request.PostAsync();

            // assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.True(response.Headers.Location.IsFile.ToString().Contains("signin"));
            
        }
        
        [Fact]
        public void should_upload_file_by_authorized_user()
        {
            // todo: add integration test for uploading a file
            
        }
        [Fact]
        public void should_download_file_by_anonymous_user()
        {
            // todo: add integration test for uploading a file
            
        }
    }
}