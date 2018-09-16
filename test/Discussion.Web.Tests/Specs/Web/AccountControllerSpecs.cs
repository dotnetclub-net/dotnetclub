using System;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Web.Controllers;
using Discussion.Web.Models;
using Discussion.Web.ViewModels;
using Jusfr.Persistent;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Web
{
    [Collection("AppSpecs")]
    public class AccountControllerSpecs
    {
        private readonly Application _myApp;
        private readonly IRepository<User> _userRepo;

        public AccountControllerSpecs(Application app)
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
            var accountCtrl = _myApp.CreateController<AccountController>();
            _userRepo.Create(new User
            {
                UserName = "jim",
                DisplayName = "Jim Green",
                HashedPassword = "111111",
                CreatedAt = DateTime.UtcNow
            });
            var userModel = new SigninUserViewModel
            {
                UserName = "jim",
                Password = "111111"
            };
            
            var sigininResult = await accountCtrl.DoSignin(userModel, null);

            Assert.True(accountCtrl.HttpContext.IsAuthenticated());
            Assert.NotNull(accountCtrl.User  as DiscussionPrincipal);
            Assert.True(accountCtrl.ModelState.IsValid);            
            Assert.Equal("Jim Green", accountCtrl.User.Identities.First().Name);

            sigininResult.IsType<RedirectResult>();
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
            Assert.Null(accountCtrl.User  as DiscussionPrincipal);
            Assert.False(accountCtrl.ModelState.IsValid);
            Assert.Equal("用户名或密码错误", accountCtrl.ModelState["UserName"].Errors.First().ErrorMessage);

            var viewResult = sigininResult as ViewResult;
            Assert.NotNull(viewResult);
            viewResult.ViewName.ShouldEqual("Signin");
        }
        
        [Fact]
        public async Task should_return_signin_view_when_incorrect_password()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            _userRepo.Create(new User
            {
                UserName = "jimwrongpwd",
                DisplayName = "Jim Green",
                HashedPassword = "11111F",
                CreatedAt = DateTime.UtcNow
            });
            var userModel = new SigninUserViewModel
            {
                UserName = "jimwrongpwd",
                Password = "11111f"
            };

            
            var sigininResult = await accountCtrl.DoSignin(userModel, null);

            Assert.False(accountCtrl.HttpContext.IsAuthenticated());
            Assert.Null(accountCtrl.User as DiscussionPrincipal);
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
        public void should_register_new_user()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            var userName = "newuser";
            var newUser = new SigninUserViewModel
            {
                UserName = userName,
                Password = "hello"
            };
            
            var registerResult = accountCtrl.DoRegister(newUser);
            registerResult.IsType<RedirectResult>();

            var registeredUser = _userRepo.All.FirstOrDefault(user => user.UserName == newUser.UserName);
            registeredUser.ShouldNotBeNull();
            // ReSharper disable once PossibleNullReferenceException
            registeredUser.UserName.ShouldEqual(userName);
            registeredUser.Id.ShouldGreaterThan(0);
        }
        
        [Fact]
        public void should_not_register_with_invalid_request()
        {
            var accountCtrl = _myApp.CreateController<AccountController>();
            var notToBeCreated = "not-to-be-created";
            var newUser = new SigninUserViewModel
            {
                UserName = notToBeCreated,
                Password = "hello"
            };
            
            accountCtrl.ModelState.AddModelError("UserName", "Some Error");
            var registerResult = accountCtrl.DoRegister(newUser);


            var userIsRegistered = _userRepo.All.Any(user => user.UserName == notToBeCreated);
            Assert.False(userIsRegistered);
            
            registerResult.IsType<ViewResult>();
            var viewResult = registerResult as ViewResult;
            // ReSharper disable once PossibleNullReferenceException
            viewResult.ViewName.ShouldEqual("Register");
        }
        
        [Fact]
        public void should_not_register_an_user_with_existing_username()
        {
            var userName = "someuser";
            _userRepo.Create(new User
            {
                UserName = userName,
                DisplayName = "old user",
                CreatedAt = new DateTime(2018, 02, 14)
            });
            var accountCtrl = _myApp.CreateController<AccountController>();

            
            var newUser = new SigninUserViewModel
            {
                UserName = userName.ToUpper(),
                Password = "hello"
            };
            var registerResult = accountCtrl.DoRegister(newUser);
            

            Assert.False(accountCtrl.ModelState.IsValid);
            accountCtrl.ModelState.Keys.ShouldContain("UserName");
            
            var allUsers = _userRepo.All.Where(user => user.UserName == userName).ToList();
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
            _myApp.MockUser();
            var accountCtrl = _myApp.CreateController<AccountController>();
            
            var signoutResult = await accountCtrl.DoSignOut();

            Assert.False(accountCtrl.User.Identity.IsAuthenticated);
            Assert.NotNull(signoutResult);
            signoutResult.IsType<RedirectResult>();
        }


    }
}