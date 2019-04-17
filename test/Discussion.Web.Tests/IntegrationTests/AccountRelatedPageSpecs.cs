using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discussion.Core.Communication.Email;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class AccountRelatedPageSpecs
    {
        readonly TestDiscussionWebApp _app;
        readonly IRepository<User> _userRepo;
        readonly UserManager<User> _userManager;
        readonly AntiForgeryRequestTokens _antiForgeryTokens;

        public AccountRelatedPageSpecs(TestDiscussionWebApp app) {
            _app = app.Reset();
            _antiForgeryTokens = _app.GetAntiForgeryTokens();
            _userRepo = _app.GetService<IRepository<User>>();
            _userManager = _app.GetService<UserManager<User>>();
        }

        #region Signin

        [Fact]
        void should_serve_signin_page_correctly()
        {
            _app.ShouldGet("/signin", responseShouldContain: "用户登录");
        }
          
        [Fact]
        void should_be_able_to_signin_new_user()
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
        void signed_in_users_should_be_able_to_view_pages_that_requires_authenticated_users()
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

        #endregion

        #region Register

        [Fact]
        void should_serve_register_page_correctly()
        {
            _app.ShouldGet("/register", responseShouldContain: "用户注册");
        }
  
        [Fact]
        void should_be_able_to_register_new_user()
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

            var isRegistered = _userRepo.All().Any(u => u.UserName == username);
            isRegistered.ShouldEqual(true);
        }

        [Fact]
        void should_signin_newly_registered_user()
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

        #endregion

        #region Forgot Password

        [Fact]
        void should_serve_forgot_password_page_correctly()
        {
            _app.ShouldGet("/forgot-password", responseShouldContain: "找回密码");
        }

        [Fact]
        void should_send_reset_password_email()
        {
            var user = CreateUser();
            var mailDeliveryMethod = new Mock<IEmailDeliveryMethod>();
            mailDeliveryMethod.Setup(sender => sender.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _app.OverrideServices(services => services.AddSingleton(mailDeliveryMethod.Object));

            _app.ShouldPost("/forgot-password",
                    new ForgotPasswordModel {UsernameOrEmail = user.UserName},
                    signinStatus: SigninRequirement.SigninNotRequired)
                .WithApiResult((api, _) => api.HasSucceeded.ShouldEqual(true));
        }

        #endregion

        #region Reset Password

        [Fact]
        void should_serve_reset_password_page_correctly()
        {
            _app.ShouldGet("/reset-password?token=123", responseShouldContain: "重置密码");
        }

        [Fact]
        async Task should_reset_password()
        {
            var user = CreateUser();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var model = new ResetPasswordModel
            {
                Token = token,
                UserId = user.Id,
                Password = "test123"
            };

            _app.ShouldPost($"/reset-password", model, SigninRequirement.SigninNotRequired)
                .WithResponse(response =>
                {
                    response.StatusCode.ShouldEqual(HttpStatusCode.OK);
                    response.ReadAllContent().ShouldContain("重置密码成功");
                });
        }

        #endregion

        User CreateUser()
        {
            var user = new User
            {
                UserName = Guid.NewGuid().ToString(),
                EmailAddress = $"{Guid.NewGuid()}@gmail.com",
                EmailAddressConfirmed = true
            };

            _userRepo.Save(user);

            return user;
        }
    }
}