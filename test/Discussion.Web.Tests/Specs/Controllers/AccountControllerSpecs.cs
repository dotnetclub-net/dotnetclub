﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discussion.Core.Communication.Email;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.ViewModels;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class AccountControllerSpecs
    {
        // ReSharper disable PossibleNullReferenceException

        readonly TestDiscussionWebApp _app;
        readonly IRepository<User> _userRepo;
        readonly UserManager<User> _userManager;

        public AccountControllerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
            _userRepo = _app.GetService<IRepository<User>>();
            _userManager = _app.GetService<UserManager<User>>();
        }

        #region Signin

        [Fact]
        void should_serve_signin_page_as_view_result()
        {
            var accountCtrl = _app.CreateController<AccountController>();

            var signinPageResult = accountCtrl.Signin(null);

            var viewResult = signinPageResult as ViewResult;
            Assert.NotNull(viewResult);
        }

        [Fact]
        async Task should_signin_user_and_redirect_when_signin_with_valid_user()
        {
            // Arrange
            ClaimsPrincipal signedInClaimsPrincipal = null;
            var authService = MockAuthService(principal => signedInClaimsPrincipal = principal);

            const string password = "111111A";
            _app.CreateUser("jim", password, "Jim Green");
            var userModel = new UserViewModel
            {
                UserName = "jim",
                Password = password
            };

            // Act
            var accountCtrl = _app.CreateController<AccountController>();
            accountCtrl.TryValidateModel(userModel);
            var signinResult = await accountCtrl.DoSignin(userModel, null);

            // Assert
            Assert.True(accountCtrl.ModelState.IsValid);
            signinResult.IsType<RedirectResult>();

            authService.Verify();
            var signedInUser = signedInClaimsPrincipal.ToDiscussionUser(_app.GetService<IRepository<User>>());
            _app.ReloadEntity(signedInUser);
            Assert.Equal("jim", signedInUser.UserName);
            Assert.NotNull(signedInUser.LastSeenAt);
        }

        Mock<IAuthenticationService> MockAuthService(Action<ClaimsPrincipal> onSignin)
        {
            var authService = new Mock<IAuthenticationService>();
            authService.Setup(auth => auth.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask)
                .Callback((HttpContext ctx, string scheme, ClaimsPrincipal claimsPrincipal, AuthenticationProperties props) =>
                {
                    onSignin(claimsPrincipal);
                })
                .Verifiable();
            _app.OverrideServices(services => services.AddSingleton(authService.Object));
            return authService;
        }

        [Fact]
        async Task should_return_signin_view_when_username_does_not_exist()
        {
            var accountCtrl = _app.CreateController<AccountController>();
            var userModel = new UserViewModel
            {
                UserName = "jimdoesnotexists",
                Password = "111111"
            };

            var sigininResult = await accountCtrl.DoSignin(userModel, null);


            Assert.False(accountCtrl.HttpContext.IsAuthenticated());
            Assert.False(accountCtrl.ModelState.IsValid);
            Assert.Equal("用户名或密码错误", accountCtrl.ModelState["UserName"].Errors.First().ErrorMessage);

            var viewResult = sigininResult as ViewResult;
            Assert.NotNull(viewResult);
            viewResult.ViewName.ShouldEqual("Signin");
        }

        [Fact]
        async Task should_return_signin_view_when_incorrect_password()
        {
            const string userName = "jimwrongpwd";
            _app.CreateUser(userName, "11111F");
            var userModel = new UserViewModel
            {
                UserName = userName,
                Password = "11111f"
            };

            var accountCtrl = _app.CreateController<AccountController>();
            var signinResult = await accountCtrl.DoSignin(userModel, null);

            Assert.False(accountCtrl.HttpContext.IsAuthenticated());
            Assert.False(accountCtrl.ModelState.IsValid);
            Assert.Equal("用户名或密码错误", accountCtrl.ModelState["UserName"].Errors.First().ErrorMessage);

            var viewResult = signinResult as ViewResult;
            Assert.NotNull(viewResult);
            viewResult.ViewName.ShouldEqual("Signin");
        }

        [Fact]
        async Task should_return_signin_view_when_invalid_signin_request()
        {
            var accountCtrl = _app.CreateController<AccountController>();
            accountCtrl.ModelState.AddModelError("UserName", "UserName is required");

            var signinResult = await accountCtrl.DoSignin(new UserViewModel(), null);

            var viewResult = signinResult as ViewResult;
            Assert.NotNull(viewResult);
            viewResult.ViewName.ShouldEqual("Signin");
        }

        #endregion

        #region Register

        [Fact]
        void should_return_register_page_as_view_result()
        {
            var accountCtrl = _app.CreateController<AccountController>();
            accountCtrl.ModelState.AddModelError("UserName", "UserName is required");

            var registerPage = accountCtrl.Register();

            var viewResult = registerPage as ViewResult;
            Assert.NotNull(viewResult);
        }

        [Fact]
        async Task should_register_new_user()
        {
            var accountCtrl = _app.CreateController<AccountController>();
            var userName = "newuser1234";
            var newUser = new UserViewModel
            {
                UserName = userName,
                Password = "hello1"
            };

            var registerResult = await accountCtrl.DoRegister(newUser);
            registerResult.IsType<RedirectResult>();

            var registeredUser = _userRepo.All().FirstOrDefault(user => user.UserName == newUser.UserName);
            registeredUser.ShouldNotBeNull();
            registeredUser.UserName.ShouldEqual(userName);
            registeredUser.Id.ShouldGreaterThan(0);
        }

        [Fact]
        async Task should_not_register_new_user_when_disabled()
        {
            _app.OverrideServices(services =>
            {
                services.AddSingleton(new SiteSettings
                {
                    IsReadonly = false,
                    EnableNewUserRegistration = false
                });
            });
            var accountCtrl = _app.CreateController<AccountController>();
            var userName = "newuser";
            var newUser = new UserViewModel
            {
                UserName = userName,
                Password = "hello1"
            };

            var registerResult = await accountCtrl.DoRegister(newUser);
            registerResult.IsType<ViewResult>();

            var registeredUser = _userRepo.All().FirstOrDefault(user => user.UserName == newUser.UserName);
            registeredUser.ShouldBeNull();
        }

        [Fact]
        async Task should_hash_password_for_user()
        {
            var accountCtrl = _app.CreateController<AccountController>();
            var userName = "user";
            var clearPassword = "password1";
            var newUser = new UserViewModel
            {
                UserName = userName,
                Password = clearPassword
            };

            var registerResult = await accountCtrl.DoRegister(newUser);
            registerResult.IsType<RedirectResult>();

            var registeredUser = _userRepo.All().FirstOrDefault(user => user.UserName == newUser.UserName);
            registeredUser.ShouldNotBeNull();
            registeredUser.UserName.ShouldEqual(userName);
            registeredUser.HashedPassword.ShouldNotEqual(clearPassword);
        }

        [Fact]
        async Task should_not_register_with_invalid_request()
        {
            var accountCtrl = _app.CreateController<AccountController>();
            var notToBeCreated = "not-to-be-created";
            var newUser = new UserViewModel
            {
                UserName = notToBeCreated,
                Password = "hello"
            };

            accountCtrl.ModelState.AddModelError("UserName", "Some Error");
            var registerResult = await accountCtrl.DoRegister(newUser);


            var userIsRegistered = _userRepo.All().Any(user => user.UserName == notToBeCreated);
            Assert.False(userIsRegistered);

            registerResult.IsType<ViewResult>();
            var viewResult = registerResult as ViewResult;
            viewResult.ViewName.ShouldEqual("Register");
        }

        [Fact]
        async Task should_not_register_an_user_with_existing_username()
        {
            const string userName = "someuser";
            _app.CreateUser(userName, displayName: "old user");
            var accountCtrl = _app.CreateController<AccountController>();


            var newUser = new UserViewModel
            {
                UserName = userName.ToUpper(),
                Password = "hello"
            };
            var registerResult = await accountCtrl.DoRegister(newUser);


            Assert.False(accountCtrl.ModelState.IsValid);
            accountCtrl.ModelState.Keys.ShouldContain("UserName");

            var allUsers = _userRepo.All().Where(user => user.UserName == userName).ToList();
            allUsers.Count.ShouldEqual(1);
            allUsers[0].DisplayName.ShouldEqual("old user");

            registerResult.IsType<ViewResult>();
            var viewResult = registerResult as ViewResult;
            viewResult.ViewName.ShouldEqual("Register");
        }

        #endregion

        #region Forgot Password

        [Fact]
        void should_return_forgot_password_page_as_view_result()
        {
            var controller = _app.CreateController<AccountController>();

            var result = controller.ForgotPassword() as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        async void should_not_send_reset_password_email_when_user_not_existed()
        {
            var model = new ForgotPasswordModel
            {
                UsernameOrEmail = Guid.NewGuid().ToString()
            };

            var controller = _app.CreateController<AccountController>();
            var result = await controller.DoForgotPassword(model);

            result.ErrorMessage.ShouldEqual("该用户不存在");
        }

        [Fact]
        async void should_not_send_reset_password_email_when_user_email_not_confirmed()
        {
            var user = CreateUser();
            var model = new ForgotPasswordModel { UsernameOrEmail = user.UserName };

            var controller = _app.CreateController<AccountController>();
            var result = await controller.DoForgotPassword(model);

            result.ErrorMessage.ShouldEqual("无法验证你对账号的所有权，因为之前没有已验证过的邮箱地址");
        }

        [Fact]
        async void should_send_reset_password_email()
        {
            var user = CreateUser(true);
            var model = new ForgotPasswordModel { UsernameOrEmail = user.UserName };

            var mailSender = MockMailSender();
            var controller = _app.CreateController<AccountController>();
            var result = await controller.DoForgotPassword(model);

            result.ShouldNotBeNull();
            result.HasSucceeded.ShouldBeTrue();

            mailSender.Verify(x => x.SendEmailAsync(
                user.EmailAddress,
                "dotnet club 用户密码重置",
                It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Reset Password

        [Fact]
        void should_return_reset_password_page_as_view_result()
        {
            var model = new ResetPasswordModel();

            var controller = _app.CreateController<AccountController>();
            var result = controller.ResetPassword(model) as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        void should_return_error_when_goto_reset_password_page_with_invalid_token()
        {
            var model = new ResetPasswordModel {Token = "hello"};

            var controller = _app.CreateController<AccountController>();
            var result = controller.ResetPassword(model);

            controller.ModelState.IsValid.ShouldBeFalse();
            controller.ModelState[nameof(model.Token)].Errors.First()
                .ErrorMessage.ShouldEqual("Token无法识别");
        }

        [Fact]
        async void should_not_reset_password_when_token_invalid()
        {
            var user = CreateUser(true);
            var model = new ResetPasswordModel
            {
                Token = "hello",
                UserId = user.Id,
                Password = "test123"
            };

            var controller = _app.CreateController<AccountController>();
            var result = await controller.DoResetPassword(model) as ViewResult;

            controller.ModelState.IsValid.ShouldBeFalse();
            controller.ModelState["InvalidToken"].Errors.First()
                .ErrorMessage.ShouldEqual("验证令牌不正确");
        }

        [Fact]
        async void should_reset_password()
        {
            var user = CreateUser(true);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var model = new ResetPasswordModel
            {
                Token = token,
                UserId = user.Id,
                Password = "test123"
            };

            var controller = _app.CreateController<AccountController>();
            var result = await controller.DoResetPassword(model) as ViewResult;

            (result.Model as ResetPasswordModel).Succeeded.ShouldBeTrue();
        }

        #endregion

        [Fact]
        async Task should_sign_out()
        {
            var authService = new Mock<IAuthenticationService>();
            authService.Setup(auth => auth.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask).Verifiable();
            _app.OverrideServices(services => services.AddSingleton(authService.Object));
            _app.MockUser();
            var accountCtrl = _app.CreateController<AccountController>();

            var signOutResult = await accountCtrl.DoSignOut();

            Assert.NotNull(signOutResult);
            signOutResult.IsType<RedirectResult>();
            authService.Verify();
        }

        Mock<IEmailDeliveryMethod> MockMailSender()
        {
            var mailSender = new Mock<IEmailDeliveryMethod>();

            mailSender.Setup(sender => sender.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _app.OverrideServices(services => services.AddSingleton(mailSender.Object));

            return mailSender;
        }

        User CreateUser(bool emailConfirmed = false)
        {
            var user = new User
            {
                UserName = Guid.NewGuid().ToString(),
                EmailAddress = $"{Guid.NewGuid()}@gmail.com",
                EmailAddressConfirmed = emailConfirmed
            };

            _userRepo.Save(user);

            return user;
        }
    }
}
