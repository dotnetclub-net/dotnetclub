using Discussion.Tests.Common;
using Discussion.Web.ViewModels;
using Xunit;

namespace Discussion.Web.Tests.Specs.ViewModels
{
    [Collection("AppSpecs")]
    public class ReplyCreationModelSpecs
    {
        private readonly TestDiscussionWebApp _app;

        public ReplyCreationModelSpecs(TestDiscussionWebApp app)
        {
            _app = app;
        }



        [Fact]
        public void should_validate_empty_content_as_invalid()
        {
            var replyModel = new ReplyCreationModel();
            
            var modelState = _app.ValidateModel(replyModel);
            
            Assert.False(modelState.IsValid);
        }
    }
}