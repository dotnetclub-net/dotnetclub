using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discussion.Core.Communication.Email;
using Discussion.Core.Communication.Sms;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.Services.ChatHistoryImporting;
using Discussion.Web.Services.UserManagement;
using Discussion.Web.Tests.Specs.Services;
using Discussion.Web.Tests.Stubs;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class UserControllerSpecs
    {
        private readonly TestDiscussionWebApp _theApp;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<PhoneNumberVerificationRecord> _phoneVerifyRepo;
        private readonly IRepository<VerifiedPhoneNumber> _phoneRepo;

        public UserControllerSpecs(TestDiscussionWebApp theApp)
        {
            _theApp = theApp;
            _userRepo = _theApp.GetService<IRepository<User>>();
            _phoneVerifyRepo = theApp.GetService<IRepository<PhoneNumberVerificationRecord>>();
            _phoneRepo = theApp.GetService<IRepository<VerifiedPhoneNumber>>();
            _theApp.DeleteAll<User>();
        }

        #region Update Profile
        
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

        #endregion
  
        #region Change Password

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
        public async Task should_not_update_password_with_wrong_password()
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
            ShouldBeViewResultWithErrors(result, userCtrl.ModelState, "密码不正确", "ChangePassword");
            var passwordChanged = await _theApp.GetService<UserManager<User>>().CheckPasswordAsync(user, viewModel.NewPassword);
            Assert.False(passwordChanged);
        }

        #endregion
        
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
            ShouldBeViewResultWithErrors(result, userCtrl.ModelState, "邮件地址格式不正确");
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
            Assert.False(appUser.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_not_bind_email_with_email_address_already_confirmed_by_another_user()
        {
            var appUser = UseUpdatedAppUser("one@before-change.com", confirmed: false);
            CreateUser("email@taken.com", confirmed: true);

            var (result, userCtrl) = await SubmitSettingsWithEmail("email@taken.com");
            
            ShouldBeViewResultWithErrors(result, userCtrl.ModelState, "邮件地址已由其他用户使用");

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
            
            
            var userCtrl = _theApp.CreateController<UserController>();
            await userCtrl.SendEmailConfirmation();


            var generatedUrl = userCtrl.GetFakeRouter().GetGeneratedUrl;
            var token = generatedUrl["token"].ToString();
            var viewResult = await userCtrl.ConfirmEmail(token);
            
            _theApp.ReloadEntity(user);
            Assert.Equal("one@changing.com", user.EmailAddress);
            Assert.True(user.EmailAddressConfirmed);
            
            Assert.NotNull(viewResult);
            Assert.True(userCtrl.ModelState.IsValid);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName));
        }

        [Fact]
        public async Task should_not_confirm_email_when_already_confirmed_by_another_user()
        {
            MockMailSender();
            var user = UseUpdatedAppUser("email@taken.com", confirmed: false);
            
            var userCtrl = _theApp.CreateController<UserController>();
            await userCtrl.SendEmailConfirmation();
            var generatedUrl = userCtrl.GetFakeRouter().GetGeneratedUrl;
            var token = generatedUrl["token"].ToString();

            CreateUser("email@taken.com", confirmed: true);
            var viewResult = await userCtrl.ConfirmEmail(token);
            var isConfirmed = (bool)viewResult.Model;

            _theApp.ReloadEntity(user);
            Assert.Equal("email@taken.com", user.EmailAddress);
            Assert.False(user.EmailAddressConfirmed);
            Assert.False(isConfirmed);
        }

        // QUESTION: should not confirm with used token?
        
        #endregion
        
        #region Phone Number Verification

        [Fact]
        public async Task should_send_sms_to_specified_phone_number()
        {
            const string phoneNumber = "13603503455";
            var smsSender = MockSmsSender(phoneNumber);
            
            var user = _theApp.MockUser();
            var userCtrl = _theApp.CreateController<UserController>();
            
            
            var result = await userCtrl.SendPhoneNumberVerificationCode(phoneNumber);
            
            Assert.True(result.HasSucceeded);
            smsSender.Verify();
            _theApp.ReloadEntity(user);
            Assert.Null(user.PhoneNumberId);

            var sentRecord = _phoneVerifyRepo.All().FirstOrDefault(r => r.UserId == user.Id);
            Assert.NotNull(sentRecord);
            Assert.NotNull(sentRecord.Code);
            Assert.Equal(phoneNumber, sentRecord.PhoneNumber);
            Assert.True( (sentRecord.Expires - DateTime.UtcNow).TotalMinutes > 5 );
        }
        
        [Fact]
        public async Task should_not_send_sms_again_in_2_minutes()
        {
            const string phoneNumber = "13603503455";
            
            var user = _theApp.MockUser();
            var record = new PhoneNumberVerificationRecord
            {
                UserId = user.Id,
                Code = "389451",
                Expires = DateTime.UtcNow.AddMinutes(15),
                PhoneNumber = phoneNumber
            };
            _phoneVerifyRepo.Save(record);
            var userCtrl = _theApp.CreateController<UserController>();
            
            var result = await userCtrl.SendPhoneNumberVerificationCode(phoneNumber);
            
            Assert.False(result.HasSucceeded);
            var sentRecords = _phoneVerifyRepo
                .All()
                .Where(r => r.UserId == user.Id && r.PhoneNumber == phoneNumber)
                .ToList();
            Assert.Equal(1, sentRecords.Count);
        }
        
        [Fact]
        public async Task should_not_send_sms_again_in_7_days_after_verified()
        {
            const string phoneNumber = "13603503455";
            
            var verifiedRecord = new VerifiedPhoneNumber {PhoneNumber = phoneNumber};
            _phoneRepo.Save(verifiedRecord);
            var user = _theApp.MockUser();
            user.VerifiedPhoneNumber = verifiedRecord;
            _userRepo.Update(user);
            
            var userCtrl = _theApp.CreateController<UserController>();
            var result = await userCtrl.SendPhoneNumberVerificationCode(phoneNumber);
            
            Assert.False(result.HasSucceeded);
            var messageSent = _phoneVerifyRepo
                .All()
                .Any(r => r.UserId == user.Id && r.PhoneNumber == phoneNumber);
            Assert.False(messageSent);
        }
        
        [Fact]
        public async Task should_not_send_sms_for_a_user_more_than_5_times_in_a_day()
        {
            const string phoneNumber = "13603503455";

            var user = _theApp.MockUser();
            Enumerable.Range(1, 5).ToList().ForEach(index =>
            {
                var record = new PhoneNumberVerificationRecord
                {
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-2 * index),
                    UserId = user.Id,
                    Code = "389451",
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    PhoneNumber = StringUtility.RandomNumbers(11)
                };
                _phoneVerifyRepo.Save(record);    
            });
            
            
            var userCtrl = _theApp.CreateController<UserController>();
            var result = await userCtrl.SendPhoneNumberVerificationCode(phoneNumber);
            
            Assert.False(result.HasSucceeded);
            var messageSent = _phoneVerifyRepo
                .All()
                .Where(r => r.UserId == user.Id)
                .ToList();
            Assert.Equal(5, messageSent.Count);
        }
        
        [Fact]
        public void should_verify_phone_number()
        {
            const string code = "389451";
            const string phoneNumber = "13603503455";
            
            var user = _theApp.MockUser();
            var record = new PhoneNumberVerificationRecord
            {
                UserId = user.Id,
                Code = code,
                Expires = DateTime.UtcNow.AddMinutes(15),
                PhoneNumber = phoneNumber
            };
            _phoneVerifyRepo.Save(record);
            
            var userCtrl = _theApp.CreateController<UserController>();
            var result = userCtrl.DoVerifyPhoneNumber(code);
            
            Assert.True(result.HasSucceeded);
            _theApp.ReloadEntity(user);
            Assert.NotNull(user.PhoneNumberId);
        }
        
        // *todo: should auto invalid after 1 year
        
        #endregion

        #region Binding WeChat Account

        [Fact]
        public async void should_get_chaty_qrcode()
        {
            var chatyApiService = new Mock<ChatyApiServiceMock>();
            chatyApiService.Setup(chaty => chaty.GetChatyBotStatus())
                .Returns(Task.FromResult(new ChatyBotInfoViewModel()
                {
                    QrCode = "some-qr-code-content",
                    Name = "Bot"
                }))
                .Verifiable();
            
            var userCtrl = new UserController(null,null, 
                _theApp.GetService<ILogger<UserController>>(), 
                _theApp.GetService<IRepository<WeChatAccount>>(),
                chatyApiService.Object);
            
            var requestResult = await userCtrl.GetChatyBotInfo();
            
            Assert.True(requestResult.HasSucceeded);
            dynamic qrCodeResult = requestResult.Result;
            string qrcodeContent = qrCodeResult.QrCode;
            string name = qrCodeResult.Name;
            
            Assert.Equal("some-qr-code-content", qrcodeContent);
            Assert.Equal("Bot", name);
            chatyApiService.Verify();
        }
         
        [Fact]
        public async void should_verify_wechat_account()
        {
            var user = _theApp.MockUser();
            var chatyApiService = new Mock<ChatyApiServiceMock>();
            chatyApiService.Setup(chaty => chaty.VerifyWeChatAccount("123456"))
                .Returns(Task.FromResult(new ChatyVerifyResultViewModel()
                {
                    Id= "verified_wx_id",
                    Name = "Bot"
                }))
                .Verifiable();
            var httpContext = new DefaultHttpContext
            {
                User = _theApp.User, 
                RequestServices = _theApp.ApplicationServices
            };
            _theApp.GetService<IHttpContextAccessor>().HttpContext = httpContext;

            var userCtrl = new UserController(null, null,
                _theApp.GetService<ILogger<UserController>>(),
                _theApp.GetService<IRepository<WeChatAccount>>(),
                chatyApiService.Object)
                .WithHttpContext(httpContext);
            
            var requestResult = await userCtrl.VerifyWeChatAccountByCode("123456");
            
            Assert.True(requestResult.HasSucceeded);
            chatyApiService.Verify();
            
            var wechatAccountRepo = _theApp.GetService<IRepository<WeChatAccount>>();
            var wechatAccount = wechatAccountRepo.All().FirstOrDefault(wx => wx.UserId == user.Id);
            
            Assert.NotNull(wechatAccount);
            Assert.Equal("verified_wx_id", wechatAccount.WxId);
            Assert.Equal(user.Id, wechatAccount.UserId);
        }
        
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
            // ReSharper disable once PossibleNullReferenceException
            redirectResult.ActionName.ShouldEqual(actionName);
        }

        private static void ShouldBeViewResultWithErrors(IActionResult result, ModelStateDictionary modelState, 
            string errorDescription, string viewName = "Settings")
        {
            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();
            // ReSharper disable once PossibleNullReferenceException
            viewResult.ViewName.ShouldEqual(viewName);
            Assert.NotNull(viewResult.Model);
            Assert.False(modelState.IsValid);
            var parsedErrors = ApiResponse.Error(modelState);
            Assert.Contains(errorDescription, parsedErrors.ErrorMessage);
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

        private Mock<IEmailDeliveryMethod> MockMailSender(bool willBeCalled = true)
        {
            var mailSender = new Mock<IEmailDeliveryMethod>();
            if (willBeCalled)
            {
                mailSender.Setup(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
            }

            _theApp.OverrideServices(services => services.AddSingleton(mailSender.Object));
            return mailSender;
        }
        
        private Mock<ISmsSender> MockSmsSender(string phoneNumber)
        {
            var smsSender = new Mock<ISmsSender>();
            smsSender.Setup(sender => sender.SendVerificationCodeAsync(phoneNumber, It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            
            _theApp.OverrideServices(services => services.AddSingleton(smsSender.Object));
            return smsSender;
        }

        #endregion
    }
}