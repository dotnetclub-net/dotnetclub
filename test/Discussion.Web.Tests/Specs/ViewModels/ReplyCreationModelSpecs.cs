using Discussion.Web.ViewModels;
using Xunit;

namespace Discussion.Web.Tests.Specs.ViewModels
{
    [Collection("AppSpecs")]
    public class ReplyCreationModelSpecs
    {
        private readonly TestApplication _myApp;

        public ReplyCreationModelSpecs(TestApplication app)
        {
            _myApp = app;
        }



        [Fact]
        public void should_validate_empty_content_as_invalid()
        {
            var replyModel = new ReplyCreationModel();
            
            var modelState = _myApp.ValidateModel(replyModel);
            
            Assert.False(modelState.IsValid);
        }
    }
}