using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class AccountRelatedPageSpecs
    {
        private TestApplication _theApp;
        public AccountRelatedPageSpecs(TestApplication theApp) {
            _theApp = theApp.Reset();
        }


        [Fact]
        public async Task should_serve_signin_page_correctly()
        {
            // arrange
            var request = _theApp.Server.CreateRequest("/signin");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Content().ShouldContain("用户登录");            
        }
        
        [Fact]
        public async Task should_serve_register_page_correctly()
        {
            // arrange
            var request = _theApp.Server.CreateRequest("/register");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Content().ShouldContain("用户注册");            
        }


    }
}