using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class AccountControllerSpecs
    {
        // ReSharper disable PossibleNullReferenceException

        private readonly TestDiscussionWebApp _app;
        private readonly IRepository<User> _userRepo;

        public AccountControllerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
            _userRepo = _app.GetService<IRepository<User>>();
        }

        #region Signin

        [Fact]
        void should_serve_signin_page_as_view_result()
        {
            var accountCtrl = _app.CreateController<AccountController>();

            IActionResult signinPageResult = accountCtrl.Signin(null);

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

        private Mock<IAuthenticationService> MockAuthService(Action<ClaimsPrincipal> onSignin)
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

        #region Retrieve Password

        [Fact]
        void should_return_retrieve_password_page_as_view_result()
        {
            var accountCtrl = _app.CreateController<AccountController>();

            var viewResult = accountCtrl.RetrievePassword() as ViewResult;

            Assert.NotNull(viewResult);
        }

        [Fact]
        void should_not_send_reset_password_email_when_user_not_existed()
        {
            var model = new RetrievePasswordModel
            {
                UsernameOrEmail = "test"
            };

            var controller = _app.CreateController<AccountController>();
            var result = controller.DoRetrievePassword(model);

            controller.ModelState.IsValid.ShouldBeFalse();
            controller.ModelState.Keys.ShouldContain("UsernameOrEmail");
        }

        [Fact]
        void should_not_send_reset_password_email_when_user_existed_but_email_not_confirmed()
        {
            var model = new RetrievePasswordModel
            {
                UsernameOrEmail = "test"
            };

            var controller = _app.CreateController<AccountController>();
            var result = controller.DoRetrievePassword(model);

            controller.ModelState.IsValid.ShouldBeFalse();
            controller.ModelState.Keys.ShouldContain("UsernameOrEmail");
        }

        [Fact]
        void should_not_send_reset_password_email_when_confirmed_email_not_existed()
        {
            var model = new RetrievePasswordModel
            {
                UsernameOrEmail = "test@gmail.com"
            };

            var controller = _app.CreateController<AccountController>();
            var result = controller.DoRetrievePassword(model);

            controller.ModelState.IsValid.ShouldBeFalse();
            controller.ModelState.Keys.ShouldContain("UsernameOrEmail");
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
    }
}
