using System;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.Services.EmailConfirmation;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
        public async Task should_bind_email_with_normal_email_address()
        {
            _theApp.MockUser();
            var user = _theApp.User.ToDiscussionUser(_userRepo);
            user.EmailAddress = "one@before-change.com";
            user.EmailAddressConfirmed = true;
            _userRepo.Update(user);
            
            var userCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "someone@changed.com" };
            var result = await userCtrl.DoSettings(emailSettingViewModel);
            
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
            
            var userCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "someone#cha.com" };
            userCtrl.ObjectValidator.Validate(userCtrl.ControllerContext,
                null,
                null,
                emailSettingViewModel);
            var result = await userCtrl.DoSettings(emailSettingViewModel);
            
            var redirectResult = result as ViewResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.ViewName.ShouldEqual("Settings");
            Assert.False(userCtrl.ModelState.IsValid);
            Assert.True(userCtrl.ModelState.Keys.Contains(nameof(EmailSettingViewModel.EmailAddress)));

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
            
            var userCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "email@taken.com" };
            var result = await userCtrl.DoSettings(emailSettingViewModel);
            
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
            _userRepo.Save(new User
            {
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                UserName = "SomeUser",
                DisplayName = "SomeUser",
                EmailAddress = "email@taken.com",
                EmailAddressConfirmed = true,
                HashedPassword = "hashed-password"
            });
            _theApp.MockUser();
            var appUser = _theApp.User.ToDiscussionUser(_userRepo);
            appUser.EmailAddress = "one@before-change.com";
            appUser.EmailAddressConfirmed = false;
            _userRepo.Update(appUser);

            var userCtrl = _theApp.CreateController<UserController>();
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "email@taken.com" };
            var result = await userCtrl.DoSettings(emailSettingViewModel);
            
            var redirectResult = result as ViewResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.ViewName.ShouldEqual("Settings");
            Assert.False(userCtrl.ModelState.IsValid);
            Assert.True(userCtrl.ModelState.Keys.Contains(nameof(EmailSettingViewModel.EmailAddress)));

            _theApp.GetService<ApplicationDbContext>().Entry(appUser).Reload();
            Assert.Equal("one@before-change.com", appUser.EmailAddress);
            Assert.False(appUser.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_send_email_confirmation_email()
        {
            _theApp.MockUser();
            var user = _theApp.User.ToDiscussionUser(_userRepo);
            user.EmailAddress = "one@changing.com";
            user.EmailAddressConfirmed = false;
            _userRepo.Update(user);

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>())).Returns("confirm-email");
            var mailSender = new Mock<IEmailSender>();
            mailSender.Setup(sender =>sender.SendEmailAsync("one@changing.com", "dotnet club 用户邮件地址确认", It.IsAny<string>()))
                        .Returns(Task.CompletedTask)
                        .Verifiable();
            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailSender.Object));
            
            var userCtrl = _theApp.CreateController<UserController>();
            userCtrl.Url = urlHelper.Object;
            var result = await userCtrl.SendEmailConfirmation();
            
            Assert.True(result.HasSucceeded);
            mailSender.Verify();

            _theApp.GetService<ApplicationDbContext>().Entry(user).Reload();
            Assert.Equal("one@changing.com", user.EmailAddress);
            Assert.False(user.EmailAddressConfirmed);
        }
 
        [Fact]
        public async Task should_not_send_email_confirmation_email_if_already_confirmed()
        {
            _theApp.MockUser();
            var user = _theApp.User.ToDiscussionUser(_userRepo);
            user.EmailAddress = "one@changing.com";
            user.EmailAddressConfirmed = true;
            _userRepo.Update(user);

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>())).Returns("confirm-email");
            var mailSender = new Mock<IEmailSender>();
            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailSender.Object));
            
            var userCtrl = _theApp.CreateController<UserController>();
            userCtrl.Url = urlHelper.Object;
            var result = await userCtrl.SendEmailConfirmation();
            
            Assert.False(result.HasSucceeded);
            mailSender.VerifyNoOtherCalls();

            _theApp.GetService<ApplicationDbContext>().Entry(user).Reload();
            Assert.Equal("one@changing.com", user.EmailAddress);
            Assert.True(user.EmailAddressConfirmed);
        }
 
        [Fact]
        public async Task should_confirm_email()
        {
            _theApp.MockUser();
            var user = _theApp.User.ToDiscussionUser(_userRepo);
            user.EmailAddress = "one@changing.com";
            user.EmailAddressConfirmed = false;
            _userRepo.Update(user);

            dynamic routeValues = null; 
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>()))
                .Callback((UrlActionContext ctx) => { routeValues = ctx.Values; })
                .Returns("confirm-email");
            var mailSender = new Mock<IEmailSender>();
            mailSender.Setup(sender =>sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailSender.Object));
            
            var userCtrl = _theApp.CreateController<UserController>();
            userCtrl.Url = urlHelper.Object;
            await userCtrl.SendEmailConfirmation();


            string token = routeValues.token;
            var result = await userCtrl.ConfirmEmail(token);
            
            _theApp.GetService<ApplicationDbContext>().Entry(user).Reload();
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
            dynamic routeValues = null; 
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>()))
                .Callback((UrlActionContext ctx) => { routeValues = ctx.Values; })
                .Returns("confirm-email");
            var mailSender = new Mock<IEmailSender>();
            mailSender.Setup(sender =>sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailSender.Object));
           
            _theApp.MockUser();
            var user = _theApp.User.ToDiscussionUser(_userRepo);
            user.EmailAddress = "email@taken.com";
            user.EmailAddressConfirmed = false;
            _userRepo.Update(user);            
            var userCtrl = _theApp.CreateController<UserController>();
            userCtrl.Url = urlHelper.Object;
            await userCtrl.SendEmailConfirmation();
            string token = routeValues.token;


            _userRepo.Save(new User
            {
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                UserName = "SomeUser",
                DisplayName = "SomeUser",
                EmailAddress = "email@taken.com",
                EmailAddressConfirmed = true,
                HashedPassword = "hashed-password"
            });
            await userCtrl.ConfirmEmail(token);
            
            _theApp.GetService<ApplicationDbContext>().Entry(user).Reload();
            Assert.Equal("email@taken.com", user.EmailAddress);
            Assert.False(user.EmailAddressConfirmed);
            Assert.False(userCtrl.ModelState.IsValid);
        }

        // QUESTION: should not confirm with used token?
    }
}