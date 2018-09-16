using System;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Web.Controllers;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Web
{
    [Collection("AppSpecs")]
    public class AccountControllerSpecs
    {
        private readonly Application _myApp;
        public AccountControllerSpecs(Application app)
        {
            _myApp = app.Reset();
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
        public async Task should_signout()
        {
            _myApp.MockUser();
            var accountCtrl = _myApp.CreateController<AccountController>();
            
            var signoutResult = await accountCtrl.DoSignOut();

            Assert.False(accountCtrl.User.Identity.IsAuthenticated);
            Assert.NotNull(signoutResult);
            Assert.IsType<RedirectResult>(signoutResult);
        }


    }
}