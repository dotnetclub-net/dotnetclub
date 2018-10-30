using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Mvc;
using Discussion.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Discussion.Admin.Tests.IntegrationSpecs
{
    [Collection("AdminSpecs")]
    public class HomePageSpecs
    {
        private TestDiscussionAdminApp _app;

        public HomePageSpecs(TestDiscussionAdminApp app)
        {
            _app = app;
        }

        [Fact]
        public async Task should_server_admin_home_page()
        {
            var request = _app.Server.CreateRequest("/admin-home");

            var response = await request.GetAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello Admin", response.ReadAllContent());
        }

        [Fact]
        public async Task should_server_admin_error_page()
        {
            var request = _app.Server.CreateRequest("/error");

            var response = await request.GetAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var jsonContent = response.ReadAllContent();
            var apiRes = JsonConvert.DeserializeObject<ApiResponse>(jsonContent);
            Assert.Equal(500, apiRes.Code);
            Assert.Equal("A server error has occured", apiRes.ErrorMessage);
        }

        [Fact]
        public async Task should_server_admin_object_result()
        {
            var request = _app.Server.CreateRequest("/ping");

            var response = await request.GetAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var jsonContent = response.ReadAllContent();
            var apiRes = JsonConvert.DeserializeObject<ApiResponse>(jsonContent);
            var result = apiRes.Result as JObject;

            Assert.Equal(200, apiRes.Code);
            Assert.NotNull(result);
            Assert.Equal("pong", result["ping"]);
        }
        
        [Fact]
        public async Task should_not_wrap_api_response_object_a_second_time()
        {
            var request = _app.Server.CreateRequest("/api-object");

            var response = await request.GetAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var jsonContent = response.ReadAllContent();
            var apiRes = JsonConvert.DeserializeObject<ApiResponse>(jsonContent);
            var result = apiRes.Result as JObject;

            Assert.Equal(200, apiRes.Code);
            Assert.NotNull(result);
            Assert.Equal("foo", result["field"]);
        }
    }
}