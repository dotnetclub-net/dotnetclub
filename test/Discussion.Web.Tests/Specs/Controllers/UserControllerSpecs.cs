using System;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.Services.EmailConfirmation;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class UserControllerSpecs
    {
        private readonly TestDiscussionWebApp _theApp;
        private readonly IRepository<User> _userRepo;

        public UserControllerSpecs(TestDiscussionWebApp theApp)
        {
            _theApp = theApp;
            _userRepo = _theApp.GetService<IRepository<User>>();
            _theApp.DeleteAll<User>();
        }

        [Fact]
        public async Task should_update_display_name()
        {
            var user = _theApp.MockUser();
            var settingViewModel = new UserSettingsViewModel
            {
                DisplayName = StringUtility.Random()
            };
            var userCtrl = _theApp.CreateController<UserController>();
            
            
            var result = await userCtrl.DoSettings(settingViewModel);

            
            ShouldBeRedirectResult(result);
            
            _theApp.ReloadEntity(user);
            Assert.Equal(settingViewModel.DisplayName, user.DisplayName);
            Assert.Null(user.EmailAddress);
            Assert.False(user.EmailAddressConfirmed);
        }
        
        [Fact]
        public async Task should_update_avatar_file_id()
        {
            var user = _theApp.MockUser();
            var settingViewModel = new UserSettingsViewModel
            {
                AvatarFileId = 12
            };
            var userCtrl = _theApp.CreateController<UserController>();
            
            
            var result = await userCtrl.DoSettings(settingViewModel);

            
            ShouldBeRedirectResult(result);
            
            _theApp.ReloadEntity(user);
            Assert.Equal(12, user.AvatarFileId);
        }
        
        [Fact]
        public async Task should_use_username_as_display_name_if_update_display_name_to_null()
        {
            var user = _theApp.MockUser();
            user.DisplayName = StringUtility.Random();
            _userRepo.Update(user);
            var settingViewModel = new UserSettingsViewModel();
            var userCtrl = _theApp.CreateController<UserController>();
            
            
            var result = await userCtrl.DoSettings(settingViewModel);

            
            ShouldBeRedirectResult(result);
            
            _theApp.ReloadEntity(user);
            Assert.Equal(user.UserName, user.DisplayName);
        }

        [Fact]
        public async Task should_update_password()
        {
            var user = _theApp.MockUser();
            var viewModel = new ChangePasswordViewModel
            {
                OldPassword = "111111",
                NewPassword = "11111A"
            };
            var userCtrl = _theApp.CreateController<UserController>();
            
            
            var result = await userCtrl.DoChangePassword(viewModel);

            
            _theApp.ReloadEntity(user);
            ShouldBeRedirectResult(result, "ChangePassword");
            var passwordChanged = await _theApp.GetService<UserManager<User>>().CheckPasswordAsync(user, viewModel.NewPassword);
            Assert.True(passwordChanged);
        }
        
        [Fact]
        public async Task should_not_update_password_with_invalid_old_password()
        {
            var user = _theApp.MockUser();
            var viewModel = new ChangePasswordViewModel
            {
                OldPassword = "111111A",
                NewPassword = "11111A"
            };
            var userCtrl = _theApp.CreateController<UserController>();
            
            
            var result = await userCtrl.DoChangePassword(viewModel);

            
            _theApp.ReloadEntity(user);
            ShouldBeViewResultWithErrors(result, userCtrl.ModelState, "ChangePassword", string.Empty);
            var passwordChanged = await _theApp.GetService<UserManager<User>>().CheckPasswordAsync(user, viewModel.NewPassword);
            Assert.False(passwordChanged);
        }
        
        
        #region Email Settings

        [Fact]
        public async Task should_bind_email_with_normal_email_address()
        {
            var user = UseUpdatedAppUser("one@before-change.com", confirmed: true);
            
            var (result, _) = await SubmitSettingsWithEmail("someone@changed.com");

            ShouldBeRedirectResult(result);

            _theApp.ReloadEntity(user);
            Assert.Equal("someone@changed.com", user.EmailAddress);
            Assert.False(user.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_not_bind_email_with_invalid_email_address()
        {
            var user = UseUpdatedAppUser("one@before-change.com", confirmed: true);


            var (result, userCtrl) = await SubmitSettingsWithEmail("someone#cha.com");
            
            _theApp.ReloadEntity(user);
            ShouldBeViewResultWithErrors(result, userCtrl.ModelState);
            Assert.Equal("one@before-change.com", user.EmailAddress);
            Assert.True(user.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_bind_email_with_email_address_if_not_confirmed_by_another_user()
        {
            var appUser = UseUpdatedAppUser("one@before-change.com", confirmed: true);
            CreateUser("email@taken.com", confirmed: false);

            var (result, _) = await SubmitSettingsWithEmail("email@taken.com");
            
            ShouldBeRedirectResult(result);

            _theApp.ReloadEntity(appUser);
            Assert.Equal("email@taken.com", appUser.EmailAddress);
            Assert.Equal(false, appUser.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_not_bind_email_with_email_address_already_confirmed_by_another_user()
        {
            var appUser = UseUpdatedAppUser("one@before-change.com", confirmed: false);
            CreateUser("email@taken.com", confirmed: true);

            var (result, userCtrl) = await SubmitSettingsWithEmail("email@taken.com");
            
            ShouldBeViewResultWithErrors(result, userCtrl.ModelState);

            _theApp.ReloadEntity(appUser);
            Assert.Equal("one@before-change.com", appUser.EmailAddress);
            Assert.False(appUser.EmailAddressConfirmed);
        }

        #endregion

        #region Email Confirmation
        
        [Fact]
        public async Task should_send_email_confirmation_email()
        {
            var mailSender = MockMailSender();
            var user = UseUpdatedAppUser("one@changing.com", confirmed: false);
            
            
            var userCtrl = _theApp.CreateController<UserController>();
            userCtrl.Url = CreateMockUrlHelper();
            var result = await userCtrl.SendEmailConfirmation();
            
            Assert.True(result.HasSucceeded);
            mailSender.Verify();

            _theApp.ReloadEntity(user);
            Assert.Equal("one@changing.com", user.EmailAddress);
            Assert.False(user.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_not_send_email_confirmation_email_if_already_confirmed()
        {
            var mailSender = MockMailSender(willBeCalled: false);
            var user = UseUpdatedAppUser("one@changing.com", confirmed: true);
            
            
            var userCtrl = _theApp.CreateController<UserController>();
            userCtrl.Url = CreateMockUrlHelper();
            var result = await userCtrl.SendEmailConfirmation();
            
            
            _theApp.ReloadEntity(user);
            
            mailSender.VerifyNoOtherCalls();
            Assert.False(result.HasSucceeded);
            Assert.Equal("one@changing.com", user.EmailAddress);
            Assert.True(user.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_confirm_email()
        {
            MockMailSender();
            var user = UseUpdatedAppUser("one@changing.com", confirmed: false);
            
            
            dynamic routeValues = null;
            var userCtrl = _theApp.CreateController<UserController>();
            userCtrl.Url = CreateMockUrlHelper(ctx => { routeValues = ctx.Values; });
            await userCtrl.SendEmailConfirmation();


            string token = routeValues.token;
            var result = await userCtrl.ConfirmEmail(token);
            
            _theApp.ReloadEntity(user);
            Assert.Equal("one@changing.com", user.EmailAddress);
            Assert.True(user.EmailAddressConfirmed);
            
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(userCtrl.ModelState.IsValid);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName));
        }

        [Fact]
        public async Task should_not_confirm_email_when_already_confirmed_by_another_user()
        {
            MockMailSender();
            var user = UseUpdatedAppUser("email@taken.com", confirmed: false);
            
            dynamic routeValues = null; 
            var userCtrl = _theApp.CreateController<UserController>();
            userCtrl.Url = CreateMockUrlHelper(ctx => { routeValues = ctx.Values; });
            await userCtrl.SendEmailConfirmation();
            string token = routeValues.token;


            CreateUser("email@taken.com", confirmed: true);
            await userCtrl.ConfirmEmail(token);
            
            _theApp.ReloadEntity(user);
            Assert.Equal("email@taken.com", user.EmailAddress);
            Assert.False(user.EmailAddressConfirmed);
            Assert.False(userCtrl.ModelState.IsValid);
        }

        // QUESTION: should not confirm with used token?
        
        #endregion

        #region Helpers

        private async Task<(IActionResult, UserController)> SubmitSettingsWithEmail(string emailAddress)
        {
            var userCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new UserSettingsViewModel { EmailAddress = emailAddress};
            userCtrl.TryValidateModel(emailSettingViewModel);
            var result = await userCtrl.DoSettings(emailSettingViewModel);
            return (result, userCtrl);
        }
        
        private static void ShouldBeRedirectResult(IActionResult result, string actionName = "Settings")
        {
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.ActionName.ShouldEqual(actionName);
        }

        private static void ShouldBeViewResultWithErrors(IActionResult result, ModelStateDictionary modelState, 
            string viewName = "Settings", string errorKey = nameof(UserSettingsViewModel.EmailAddress))
        {
            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();
            viewResult.ViewName.ShouldEqual(viewName);
            Assert.NotNull(viewResult.Model);
            Assert.False(modelState.IsValid);
            Assert.True(modelState.Keys.Contains(errorKey));
        }

        private void CreateUser(string email, bool confirmed)
        {
            _userRepo.Save(new User
            {
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                UserName = "SomeUser",
                DisplayName = "SomeUser",
                EmailAddress = email,
                EmailAddressConfirmed = confirmed,
                HashedPassword = "hashed-password"
            });
        }

        private User UseUpdatedAppUser(string newEmail, bool confirmed)
        {
            var user = _theApp.MockUser(); 
            user.EmailAddress = newEmail;
            user.EmailAddressConfirmed = confirmed;
            _userRepo.Update(user);
            return user;
        }
        
        private static IUrlHelper CreateMockUrlHelper(Action<UrlActionContext> callback = null)
        {
            var urlHelper = new Mock<IUrlHelper>();
            var setup = urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>()));
            if (callback != null)
            {
                setup.Callback(callback).Returns("confirm-email");
            }
            else
            {
                setup.Returns("confirm-email");
            }
            return urlHelper.Object;
        }

        private Mock<IEmailSender> MockMailSender(bool willBeCalled = true)
        {
            var mailSender = new Mock<IEmailSender>();
            if (willBeCalled)
            {
                mailSender.Setup(sender => sender.SendEmailAsync(It.IsAny<string>(), "dotnet club 用户邮件地址确认", It.IsAny<string>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
            }

            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailSender.Object));
            return mailSender;
        }

        #endregion
    }
}