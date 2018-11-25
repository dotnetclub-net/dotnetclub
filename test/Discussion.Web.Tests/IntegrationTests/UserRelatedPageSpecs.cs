using System.Threading.Tasks;
using Discussion.Core.Communication.Email;
using Discussion.Core.Communication.Sms;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static Discussion.Tests.Common.SigninRequirement;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class UserRelatedPageSpecs
    {
        private readonly TestDiscussionWebApp _app;
        public UserRelatedPageSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
            _app.DeleteAll<User>();
        }
        
        
        [Fact]
        public void should_show_settings_page()
        {
            _app.ShouldGet("/user/settings", 
                SigninRequired, 
                responseShouldContain: "基本信息");
        }

        [Fact]
        public void should_update_settings()
        {
            _app.ShouldPost("/user/settings", 
                new
                {
                    DisplayName = "三毛",
                    EmailAddress = "one@here.com"
                },
                SigninRequired);
        }
        
        [Fact]
        public void should_show_change_password_page()
        {
            _app.ShouldGet("/user/change-password", 
                SigninRequired, 
                responseShouldContain: "修改密码");
        }
        
        [Fact]
        public void should_change_password_for_an_authorized_user()
        {   
            _app.ShouldPost("/user/change-password", 
                new
                {
                    OldPassword = "111111",
                    NewPassword = "123423LKJLK"
                },
                SigninRequired);
        }

        [Fact]
        public void should_send_email_confirmation()
        {
            var mailDeliveryMethod = new Mock<IEmailDeliveryMethod>();
            mailDeliveryMethod.Setup(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailDeliveryMethod.Object));

            _app.ShouldPost("/user/send-confirmation-mail", 
                    signinStatus: SigninRequired)
                .WithApiResult((api, _)=> api.HasSucceeded.ShouldEqual(true));
        }
        
        [Fact]
        public void should_accept_email_confirmation_request_without_login()
        {
            _app.ShouldGet("/user/confirm-email?token=985473", 
                    signinStatus: SigninNotRequired,
                    responseShouldContain: "无法确认邮件地址");
        }
        
        [Fact]
        public void should_show_phone_number_verification_code_page()
        {
            _app.ShouldGet("/user/phone-number-verification", 
                    signinStatus: SigninRequired,
                    responseShouldContain: "手机验证");
        }
        
        [Fact]
        public void should_send_phone_number_verification_code()
        {
            var mailSender = new Mock<ISmsSender>();
            mailSender.Setup(sender => sender.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailSender.Object));

            _app.ShouldPost("/user/phone-number-verification/send-code", 
                    new {phoneNumber = "15801234567"},
                    signinStatus: SigninRequired)
                .WithApiResult((api, _)=> api.HasSucceeded.ShouldEqual(true));
        }
        
        [Fact]
        public void should_accept_verify_phone_number_request()
        {
            _app.ShouldPost("/user/phone-number-verification/verify",
                    new {code = "945323"},
                    signinStatus: SigninRequired,
                    responseShouldContain: "验证码不正确或已过期")
                .WithApiResult((api, _) => api.HasSucceeded.ShouldEqual(false));
        }
        
  
    }
}