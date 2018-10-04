using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Web.Controllers;
using Discussion.Web.Services.Identity;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("AppSpecs")]
    public class AccountControllerSpecs
    {
        private readonly TestApplication _myApp;
        private readonly IRepository<User> _userRepo;

        public AccountControllerSpecs(TestApplication app)
        {
            _myApp = app.Reset();
            _userRepo = _myApp.GetService<IRepository<User>>();
        }
        
        [Fact]
        public void should_serve_signin_page_as_view_result()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();

            IActionResult signinPageResult = accountCtrl.Signin(null);

            var viewResult = signinPageResult as ViewResult;
            Assert.NotNull(viewResult);
        }
        
        
        [Fact]
        public async Task should_signin_user_and_redirect_when_signin_with_valid_user()
        {
            // Arrange
             ClaimsPrincipal signedInClaimsPrincipal = null;
             var authService = new Mock<IAuthenticationService>();
             authService.Setup(auth => auth.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                 .Returns(Task.CompletedTask)
                 .Callback((HttpContext ctx, string scheme, ClaimsPrincipal claimsPrincipal, AuthenticationProperties props) =>
                 {
                     signedInClaimsPrincipal = claimsPrincipal;
                 })
                .Verifiable();
            ReplacableServiceProvider.Replace(services =>
                {
                    services.AddSingleton(authService.Object);
                });
            
            var accountCtrl = _myApp.CreateController<AccountController>();
            var userRepo = _myApp.GetService<IRepository<User>>();

            const string password = "111111";
            _myApp.CreateUser("jim", password, "Jim Green");
            
            // Act
            var userModel = new SigninUserViewModel
            {
                UserName = "jim",
                Password = password
            };
            var sigininResult = await accountCtrl.DoSignin(userModel, null);

            // Assert
            Assert.True(accountCtrl.ModelState.IsValid);            
            sigininResult.IsType<RedirectResult>();
            
            authService.Verify();
            Assert.Equal("jim", signedInClaimsPrincipal.ToDiscussionUser(userRepo).UserName);
        }
        
        [Fact]
        public async Task should_return_signin_view_when_username_does_not_exist()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            var userModel = new SigninUserViewModel
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
        public async Task should_return_signin_view_when_incorrect_password()
        {
            var passwordHasher = _myApp.GetService<IPasswordHasher<User>>();
            var accountCtrl = _myApp.CreateController<AccountController>();
            _userRepo.Save(new User
            {
                UserName = "jimwrongpwd",
                DisplayName = "Jim Green",
                HashedPassword = passwordHasher.HashPassword(null, "11111F"),
                CreatedAtUtc = DateTime.UtcNow
            });
            var userModel = new SigninUserViewModel
            {
                UserName = "jimwrongpwd",
                Password = "11111f"
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
        public async Task should_return_signin_view_when_invalid_signin_request()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            accountCtrl.ModelState.AddModelError("UserName", "UserName is required");

            var sigininResult = await accountCtrl.DoSignin(new SigninUserViewModel(), null);

            var viewResult = sigininResult as ViewResult;
            Assert.NotNull(viewResult);
            viewResult.ViewName.ShouldEqual("Signin");
        }
        
        [Fact]
        public void should_return_register_page_as_viewresult()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            accountCtrl.ModelState.AddModelError("UserName", "UserName is required");

            var registerPage = accountCtrl.Register();

            var viewResult = registerPage as ViewResult;
            Assert.NotNull(viewResult);
        }
        
        [Fact]
        public async Task should_register_new_user()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            var userName = "newuser";
            var newUser = new SigninUserViewModel
            {
                UserName = userName,
                Password = "hello1"
            };
            
            var registerResult = await accountCtrl.DoRegister(newUser);
            registerResult.IsType<RedirectResult>();

            var registeredUser = _userRepo.All().FirstOrDefault(user => user.UserName == newUser.UserName);
            registeredUser.ShouldNotBeNull();
            // ReSharper disable once PossibleNullReferenceException
            registeredUser.UserName.ShouldEqual(userName);
            registeredUser.Id.ShouldGreaterThan(0);
        }
                
        [Fact]
        public async Task should_hash_password_for_user()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            var userName = "user";
            var clearPassword = "password1";
            var newUser = new SigninUserViewModel
            {
                UserName = userName,
                Password = clearPassword
            };
            
            var registerResult = await accountCtrl.DoRegister(newUser);
            registerResult.IsType<RedirectResult>();

            var registeredUser = _userRepo.All().FirstOrDefault(user => user.UserName == newUser.UserName);
            registeredUser.ShouldNotBeNull();
            // ReSharper disable once PossibleNullReferenceException
            registeredUser.UserName.ShouldEqual(userName);
            registeredUser.HashedPassword.ShouldNotEqual(clearPassword);
        }
        
        [Fact]
        public async Task should_not_register_with_invalid_request()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            var notToBeCreated = "not-to-be-created";
            var newUser = new SigninUserViewModel
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
            // ReSharper disable once PossibleNullReferenceException
            viewResult.ViewName.ShouldEqual("Register");
        }
        
        [Fact]
        public async Task should_not_register_an_user_with_existing_username()
        {
            var userName = "someuser";
            _userRepo.Save(new User
            {
                UserName = userName,
                DisplayName = "old user",
                CreatedAtUtc = new DateTime(2018, 02, 14)
            });
            var accountCtrl = _myApp.CreateController<AccountController>();

            
            var newUser = new SigninUserViewModel
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
            // ReSharper disable once PossibleNullReferenceException
            viewResult.ViewName.ShouldEqual("Register");
        }
        
        
        [Fact]
        public async Task should_signout()
        {
            var authService = new Mock<IAuthenticationService>();
            authService.Setup(auth => auth.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask).Verifiable();
            ReplacableServiceProvider.Replace(services =>
            {
                services.AddSingleton(authService.Object);
            });
            _myApp.MockUser();
            var accountCtrl = _myApp.CreateController<AccountController>();
            
            var signoutResult = await accountCtrl.DoSignOut();

            Assert.NotNull(signoutResult);
            signoutResult.IsType<RedirectResult>();
            authService.Verify();
        }


    }
}