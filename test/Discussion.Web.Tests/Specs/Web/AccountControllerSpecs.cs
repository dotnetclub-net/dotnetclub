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
            var userModel = new SigninUserViewModel
            {
                UserName = "jim",
                Password = "111111"
            };
            
            var sigininResult = await accountCtrl.DoSignin(userModel, null);

            Assert.NotNull(_myApp.User);
            Assert.NotNull(accountCtrl.User);
            Assert.Equal("jim", accountCtrl.User.Identities.First().Name);
            
            Assert.NotNull(sigininResult);
            Assert.IsType<RedirectResult>(sigininResult);
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
                UserName = userName,
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