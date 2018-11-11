using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Services.EmailConfirmation;
using Discussion.Web.Tests.Specs.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class UserRelatedPageSpecs
    {
        private readonly TestDiscussionWebApp _app;
        public UserRelatedPageSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }
        
        
        [Fact]
        public async Task should_show_settings_page_for_an_authorized_user()
        {
            _app.MockUser();

            await ShouldBeAbleToRequest("/user/settings", "基本信息");
        }

        [Fact]
        public async Task should_update_settings_for_an_authorized_user()
        {
            _app.MockUser();

            await ShouldBeAbleToRequest("/user/settings", "", HttpMethod.Post, 
                new Dictionary<string, string>()
                {
                    {"DisplayName", "三毛"},
                    {"EmailAddress", "one@here.com"}
                });
        }
        
        [Fact]
        public async Task should_not_show_settings_page_for_an_unauthorized_user()
        {
            await ShouldBeBlockedWithSignInRedirect("/user/settings");
        }

        [Fact]
        public async Task should_show_change_password_page_for_an_authorized_user()
        {
            _app.MockUser();

            await ShouldBeAbleToRequest("/user/change-password", "修改密码");
        }
        
        [Fact]
        public async Task should_change_password_for_an_authorized_user()
        {
            _app.MockUser();

            await ShouldBeAbleToRequest("/user/change-password", "", HttpMethod.Post, 
                new Dictionary<string, string>()
                {
                    {"OldPassword", "111111"},
                    {"NewPassword", "123423LKJLK"}
                });
        }

        [Fact]
        public async Task should_not_show_change_password_page_for_an_unauthorized_user()
        {
            await ShouldBeBlockedWithSignInRedirect("/user/change-password");
        }

        [Fact]
        public async Task should_send_email_confirmation_for_an_authorized_user()
        {
            var mailSender = new Mock<IEmailSender>();
            mailSender.Setup(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            ReplacableServiceProvider.Replace(services => services.AddSingleton(mailSender.Object));
            _app.MockUser();

            await ShouldBeAbleToRequest("/user/send-confirmation-mail", "\"hasSucceeded\":true", HttpMethod.Post, new Dictionary<string, string>());
        }

        [Fact]
        public async Task should_not_send_email_confirmation_for_an_unauthorized_user()
        {
            await ShouldBeBlockedWithSignInRedirect("/user/send-confirmation-mail", HttpMethod.Post, new Dictionary<string, string>());
        }
        
        [Fact]
        public async Task should_accept_email_confirmation_request_without_login()
        {
            await ShouldBeAbleToRequest("/user/confirm-email?token=985473", "无法确认邮件地址");
        }
        
        
        private async Task ShouldBeAbleToRequest(string url, string responseContain, HttpMethod method = HttpMethod.Get, Dictionary<string, string> postData = null)
        {
            var response = await Request(url, method, postData);
            
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                response.Headers.Location.ToString().Contains("signin").ShouldEqual(false);
            }
            else
            {
                response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            }
            response.ReadAllContent().ShouldContain(responseContain);
        }

        private async Task ShouldBeBlockedWithSignInRedirect(string url, HttpMethod method = HttpMethod.Get, Dictionary<string, string> postData = null)
        {
            var response = await Request(url, method, postData);
            response.StatusCode.ShouldEqual(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Contains("signin").ShouldEqual(true);
        }
        
        private async Task<HttpResponseMessage> Request(string url, HttpMethod method = HttpMethod.Get, Dictionary<string, string> postData = null)
        {
            var request = _app.Server.CreateRequest(url);

            HttpResponseMessage response;
            if (method == HttpMethod.Post && postData != null)
            {
                response = await _app.RequestAntiForgeryForm(url, postData).PostAsync();
            }
            else
            {
                response = await request.GetAsync();   
            }

            return response;
        }
    }
}