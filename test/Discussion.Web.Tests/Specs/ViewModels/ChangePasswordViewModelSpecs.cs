using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.ViewModels;
using Xunit;

namespace Discussion.Web.Tests.Specs.ViewModels
{
    [Collection("WebSpecs")]
    public class ChangePasswordViewModelSpecs
    {
        private readonly TestDiscussionWebApp _app;
        public ChangePasswordViewModelSpecs(TestDiscussionWebApp app)
        {
            _app = app;
        }
        
        
        [Fact]
        public void should_require_old_password_and_new_password()
        {
            var viewModel = new ChangePasswordViewModel();
            
            var modelState = _app.ValidateModel(viewModel);
            
            Assert.False(modelState.IsValid);
            modelState.Keys.ShouldContain("OldPassword");
            modelState.Keys.ShouldContain("NewPassword");
        }
        
        [Theory]
        [InlineData("passWord", true)]
        [InlineData("p@ssw0rd", true)]
        [InlineData("11111a", true)]
        [InlineData("password", false)]
        [InlineData("111111", false)]
        [InlineData("1111a", false)]
        [InlineData("F1111", false)]
        [InlineData("1111G", false)]
        [InlineData("*&@%~%!", false)]
        public void should_validate_new_password_using_password_rules(string pwd, bool valid)
        {
            var viewModel = new ChangePasswordViewModel{OldPassword = "9029384AB", NewPassword = pwd};
            
            var modelState = _app.ValidateModel(viewModel);
            
            modelState.IsValid.ShouldEqual(valid);
            if (!valid)
            {
                modelState.Keys.ShouldContain("NewPassword");
            }
        }
        
    }
}