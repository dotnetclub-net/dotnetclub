using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Newtonsoft.Json;
using Xunit;

namespace Discussion.Admin.Tests.IntegrationSpecs
{
    [Collection("AdminSpecs")]
    public class AccountApiSpecs
    {
        
        private TestDiscussionAdminApp _app;

        public AccountApiSpecs(TestDiscussionAdminApp app)
        {
            _app = app;
        }
        
        [Fact]
        public async Task should_serve_register_api()
        {
            _app.DeleteAll<AdminUser>();
            var request = _app.Server.CreateRequest("/api/account/register");

            var response = await request
                                    .WithJson(new {userName = StringUtility.Random(), password = "password1"})
                                    .PostAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var jsonContent = response.ReadAllContent();
            var apiRes = JsonConvert.DeserializeObject<ApiResponse>(jsonContent);
            Assert.Equal(200, apiRes.Code);
        }
        
        
        [Fact]
        public async Task should_serve_signin_api()
        {
            _app.DeleteAll<AdminUser>();
            var request = _app.Server.CreateRequest("/api/account/signin");

            var response = await request
                                    .WithJson(new {userName = StringUtility.Random(), password = "password1"})
                                    .PostAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var jsonContent = response.ReadAllContent();
            var apiRes = JsonConvert.DeserializeObject<ApiResponse>(jsonContent);
            Assert.Equal(400, apiRes.Code);
        }
    }
}