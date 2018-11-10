using System;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class UserControllerSpecs
    {
        private readonly TestDiscussionWebApp _theApp;
        private IRepository<User> _userRepo;

        public UserControllerSpecs(TestDiscussionWebApp theApp)
        {
            _theApp = theApp;
            _userRepo = _theApp.GetService<IRepository<User>>();
        }

        [Fact]
        public async Task should_bind_email_with_normal_email_address()
        {
            _theApp.MockUser();
            var user = _theApp.User.ToDiscussionUser(_userRepo);
            user.EmailAddress = "one@before-change.com";
            user.EmailAddressConfirmed = true;
            _userRepo.Update(user);
            
            var accountCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "someone@changed.com" };
            var result = await accountCtrl.DoSettings(emailSettingViewModel);
            
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.ActionName.ShouldEqual("Settings");

            _theApp.GetService<ApplicationDbContext>().Entry(user).Reload();
            Assert.Equal("someone@changed.com", user.EmailAddress);
            Assert.False(user.EmailAddressConfirmed);
        }
        
        [Fact]
        public async Task should_not_bind_email_with_invalid_email_address()
        {
            _theApp.MockUser();
            var user = _theApp.User.ToDiscussionUser(_userRepo);
            user.EmailAddress = "one@before-change.com";
            user.EmailAddressConfirmed = true;
            _userRepo.Update(user);
            
            var accountCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "someone#cha.com" };
            var result = await accountCtrl.DoSettings(emailSettingViewModel);
            
            var redirectResult = result as ViewResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.ViewName.ShouldEqual("Settings");
            Assert.False(accountCtrl.ModelState.IsValid);
            Assert.True(accountCtrl.ModelState.Keys.Contains(nameof(EmailSettingViewModel.EmailAddress)));

            _theApp.GetService<ApplicationDbContext>().Entry(user).Reload();
            Assert.Equal("one@before-change.com", user.EmailAddress);
            Assert.True(user.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_bind_email_with_email_address_if_not_confirmed_by_another_user()
        {
            _theApp.MockUser();
            var appUser = _theApp.User.ToDiscussionUser(_userRepo);
            appUser.EmailAddress = "one@before-change.com";
            appUser.EmailAddressConfirmed = true;
            _userRepo.Update(appUser);
            _userRepo.Save(new User
            {
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                UserName = "SomeUser",
                DisplayName = "SomeUser",
                EmailAddress = "email@taken.com",
                EmailAddressConfirmed = false,
                HashedPassword = "hashed-password"
            });
            
            var accountCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "email@taken.com" };
            var result = await accountCtrl.DoSettings(emailSettingViewModel);
            
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.ActionName.ShouldEqual("Settings");

            _theApp.GetService<ApplicationDbContext>().Entry(appUser).Reload();
            Assert.Equal("email@taken.com", appUser.EmailAddress);
            Assert.Equal(false, appUser.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_not_bind_email_with_email_address_already_confirmed_by_another_user()
        {
            _theApp.MockUser();
            var appUser = _theApp.User.ToDiscussionUser(_userRepo);
            appUser.EmailAddress = "one@before-change.com";
            appUser.EmailAddressConfirmed = false;
            _userRepo.Update(appUser);
            _userRepo.Save(new User
            {
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                UserName = "SomeUser",
                DisplayName = "SomeUser",
                EmailAddress = "email@taken.com",
                EmailAddressConfirmed = true,
                HashedPassword = "hashed-password"
            });
            
            var accountCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "email@taken.com" };
            var result = await accountCtrl.DoSettings(emailSettingViewModel);
            
            var redirectResult = result as ViewResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.ViewName.ShouldEqual("Settings");
            Assert.False(accountCtrl.ModelState.IsValid);
            Assert.True(accountCtrl.ModelState.Keys.Contains(nameof(EmailSettingViewModel.EmailAddress)));

            _theApp.GetService<ApplicationDbContext>().Entry(appUser).Reload();
            Assert.Equal("one@before-change.com", appUser.EmailAddress);
            Assert.False(appUser.EmailAddressConfirmed);
        }


        /*
         * test cases:
         *      should_bind_email_with_normal_email_address
         *      should_not_bind_email_with_invalid_email_address
         *      should_not_bind_email_with_email_address_already_confirmed_by_another_user
         *      should_bind_email_with_email_address_if_not_confirmed_by_another_user
               should_send_email_on_send_confirmation_email
               should_not_send_on_already_confirmed
               should confirm email address
               should not confirm email address if email address token & confirmed
         */

    }
}