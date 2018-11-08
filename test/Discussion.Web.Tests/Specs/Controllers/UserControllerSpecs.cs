using System.Threading.Tasks;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    public class UserControllerSpecs
    {
        private readonly TestDiscussionWebApp _theApp;

        public UserControllerSpecs(TestDiscussionWebApp theApp)
        {
            _theApp = theApp;
        }

        [Fact]
        public async Task should_bind_email_with_normal_email_address()
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>())).Returns("confirm-email");
            _theApp.MockUser();
            
            var accountCtrl = _theApp.CreateController<UserController>();
            accountCtrl.Url = urlHelper.Object;
            
            var emailSettingViewModel = new EmailSettingViewModel { EmailAddress = "someone@qq.com" };
            var result = await accountCtrl.DoSettings(emailSettingViewModel);
            
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.ActionName.ShouldEqual("Setting");
        }
        
        // todo: test cases for other actions
    }
}