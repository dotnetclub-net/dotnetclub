using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Services.EmailConfirmation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

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
            _app.Path("/user/settings")
                .Get()
                .ShouldSuccess(_app.MockUser())
                .WithResponse(res => res.ReadAllContent().Contains("基本信息"))
                .And
                .ShouldFail(user: null)
                .WithSigninRedirect();
        }

        [Fact]
        public void should_update_settings()
        {
            _app.Path("/user/settings")
                .Post()
                .WithForm(new
                {
                    DisplayName = "三毛",
                    EmailAddress = "one@here.com"
                })
                .ShouldSuccessWithRedirect(_app.MockUser())
                .And
                .ShouldFail(user: null)
                .WithSigninRedirect();
        }
        
        [Fact]
        public void should_show_change_password_page()
        {
            _app.Path("/user/change-password")
                .Get()
                .ShouldSuccess(_app.MockUser())
                .WithResponse(res => res.ReadAllContent().Contains("修改密码"))
                .And
                .ShouldFail(user: null)
                .WithSigninRedirect();
        }
        
        [Fact]
        public void should_change_password_for_an_authorized_user()
        {
            _app.Path("/user/change-password")
                .Post()
                .WithForm(new
                {
                    OldPassword = "111111",
                    NewPassword = "123423LKJLK"
                })
                .ShouldSuccessWithRedirect(_app.MockUser())
                .And
                .ShouldFail(user: null)
                .WithSigninRedirect();
        }

        [Fact]
        public void should_send_email_confirmation()
        {
            var mailSender = new Mock<IEmailSender>();
            mailSender.Setup(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailSender.Object));

            _app.Path("/user/send-confirmation-mail")
                .Post()
                .ShouldSuccess(_app.MockUser())
                .WithApiResult((api, _)=> api.HasSucceeded.ShouldEqual(true))
                .And
                .ShouldFail(user: null)
                .WithSigninRedirect();
        }
        
        [Fact]
        public void should_accept_email_confirmation_request_without_login()
        {
            _app.Path("/user/confirm-email?token=985473")
                .Get()
                .ShouldSuccess(user: null)
                .WithResponse(res => res.ReadAllContent().Contains("无法确认邮件地址"));
        }
        
  
    }
}