using System.Linq;
using System.Net.Http;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Xunit;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class AccountRelatedPageSpecs
    {
        private readonly TestDiscussionWebApp _app;
        private readonly AntiForgeryRequestTokens _antiForgeryTokens;

        public AccountRelatedPageSpecs(TestDiscussionWebApp app) {
            _app = app.Reset();
            _antiForgeryTokens = _app.GetAntiForgeryTokens();
        }


        [Fact]
        public void should_serve_signin_page_correctly()
        {
            _app.ShouldGet("/signin", responseShouldContain: "用户登录");
        }

        [Fact]
        public void should_be_able_to_signin_new_user()
        {
            var username = StringUtility.Random();
            var password = "11111a";
            _app.CreateUser(username, password);

            _app.Path("/signin")
                .Post()
                .WithForm(new
                {
                    UserName = username,
                    Password = password
                })
                .ShouldSuccessWithRedirect(_app.NoUser())
                .WithResponse(response =>
                {
                    var cookieHeaders = response.Headers.GetValues("Set-Cookie").ToList();
                    cookieHeaders.ShouldContain(cookie => cookie.Contains(".AspNetCore.Identity.Application"));
                });
        }

        [Fact]
        public void signed_in_users_should_be_able_to_view_pages_that_requires_authenticated_users()
        {
            // arrange
            var username = StringUtility.Random();
            var password = "11111a";
            _app.CreateUser(username, password);

            HttpResponseMessage signinResponse = null;
            _app.Path("/signin")
                .Post()
                .WithForm(new
                {
                    UserName = username,
                    Password = password
                })
                .ShouldSuccessWithRedirect(_app.NoUser())
                .WithResponse(res => signinResponse = res);

            _app.Path("/topics/create")
                .Get()
                .WithCookieFrom(signinResponse)
                .ShouldSuccess()
                .WithResponse(res => res.ReadAllContent().Contains("注销"));
        }

        [Fact]
        public void should_serve_register_page_correctly()
        {
            _app.ShouldGet("/register", responseShouldContain: "用户注册");
        }

        [Fact]
        public void should_be_able_to_register_new_user()
        {
            var username = StringUtility.Random();
            var password = "11111a";

            _app.Path("/register")
                .Post()
                .WithForm(new
                {
                    UserName = username,
                    Password = password
                })
                .ShouldSuccessWithRedirect(_app.NoUser());

            var isRegistered = _app.GetService<IRepository<User>>().All().Any(u => u.UserName == username);
            isRegistered.ShouldEqual(true);
        }

        [Fact]
        public void should_signin_newly_registered_user()
        {
            HttpResponseMessage registerResponse = null;

            _app.Path("/register")
                .Post()
                .WithForm(new
                {
                    UserName = StringUtility.Random(),
                    Password = "11111a"
                })
                .ShouldSuccessWithRedirect(_app.NoUser())
                .WithResponse(res => registerResponse = res);

            _app.Path("/topics/create")
                .Get()
                .WithCookieFrom(registerResponse)
                .ShouldSuccess()
                .WithResponse(res => res.ReadAllContent().Contains("注销"));
        }

        [Fact]
        public void should_serve_retrieve_password_page_correctly()
        {
            _app.ShouldGet("/retrieve-password", responseShouldContain: "找回密码");
        }
    }
}
