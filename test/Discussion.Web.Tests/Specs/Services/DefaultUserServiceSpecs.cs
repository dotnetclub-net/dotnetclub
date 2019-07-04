using System;
using System.Threading.Tasks;
using Discussion.Core.Communication.Email;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Discussion.Web.Services;
using Discussion.Web.Services.UserManagement;
using Discussion.Web.Services.UserManagement.EmailConfirmation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    [Collection("WebSpecs")]
    public class DefaultUserServiceSpecs
    {
        private readonly TestDiscussionWebApp _app;

        public DefaultUserServiceSpecs(TestDiscussionWebApp app)
        {
            _app = app;
        }

        [Fact]
        public void should_resolve_default_user_service()
        {
            ProvideUrlHelper(new Mock<IUrlHelper>().Object);
            var userService = _app.GetService<IUserService>();
            
            Assert.NotNull(userService);
            Assert.IsType<DefaultUserService>(userService);
        }

        [Fact]
        public async Task should_not_send_forgot_password_email_when_external_auth_enabled()
        {
            var externalIdpEnabledOptions = new Mock<IOptions<IdentityServerOptions>>();
            externalIdpEnabledOptions.Setup(op => op.Value).Returns(new IdentityServerOptions {IsEnabled = true});
            
            var (mockDeliveryMethod, mockUrlHelper, mockForgotPasswordEmailBuilder) = CreateRelatedMockItems();
            var userService = new DefaultUserService(externalIdpEnabledOptions.Object, 
                null, _app.GetService<UserManager<User>>(), 
                mockDeliveryMethod.Object, mockUrlHelper.Object, null, mockForgotPasswordEmailBuilder.Object,
                null, null, null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await userService.SendEmailRetrievePasswordAsync(new User(), "https");
            });
            
            mockDeliveryMethod.VerifyNoOtherCalls();
            mockUrlHelper.VerifyNoOtherCalls();
            mockUrlHelper.VerifyNoOtherCalls();
        }

        private void ProvideUrlHelper(IUrlHelper urlHelper)
        {
            var mockActionAccessor = new Mock<IActionContextAccessor>();
            var httpContext = new DefaultHttpContext();
            httpContext.Items[typeof(IUrlHelper)] = urlHelper;
            mockActionAccessor.Setup(x => x.ActionContext).Returns(new ActionContext {HttpContext = httpContext});
            _app.OverrideServices(s => s.AddSingleton(mockActionAccessor.Object));
        }

        private static (Mock<IEmailDeliveryMethod> mockDeliveryMethod, Mock<IUrlHelper> mockUrlHelper, Mock<IResetPasswordEmailBuilder> mockEmailBuilder) CreateRelatedMockItems()
        {
            var mockDeliveryMethod = new Mock<IEmailDeliveryMethod>();
            mockDeliveryMethod.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>())).Verifiable();

            var mockEmailBuilder = new Mock<IResetPasswordEmailBuilder>();
            mockEmailBuilder.Setup(buider => buider.BuildEmailBody(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            return (mockDeliveryMethod, mockUrlHelper, mockEmailBuilder);
        }
    }
}