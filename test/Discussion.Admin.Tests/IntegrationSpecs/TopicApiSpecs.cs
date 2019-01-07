using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discussion.Core.Mvc;
using Discussion.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Discussion.Admin.Tests.IntegrationSpecs
{
    [Collection("AdminSpecs")]
    public class TopicApiSpecs
    {
        private TestDiscussionAdminApp _app;

        public TopicApiSpecs(TestDiscussionAdminApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public async Task should_authorize_admin_identity()
        {
            var request = _app.Server.CreateRequest("/api/topics");

            
            var response = await request.GetAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            
            var jsonContent = response.ReadAllContent();
            var apiRes = JsonConvert.DeserializeObject<ApiResponse>(jsonContent);

            Assert.Equal(401, apiRes.Code);
        }
        
        [Fact]
        public async Task should_allow_access_by_admin_user()
        {
            _app.MockAdminUser();
            var request = _app.Server.CreateRequest("/api/topics/999");


            var response = await request.SendAsync(HttpMethod.Delete.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            
            var jsonContent = response.ReadAllContent();
            var apiRes = JsonConvert.DeserializeObject<ApiResponse>(jsonContent);

            Assert.Equal(404, apiRes.Code);
        }
    }
}