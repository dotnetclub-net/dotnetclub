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
    }
}