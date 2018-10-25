using System.Net;
using System.Threading.Tasks;
using Discussion.Tests.Common;
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
    }
}