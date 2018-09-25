using Discussion.Web.ViewModels;
using Xunit;

namespace Discussion.Web.Tests.Specs.ViewModels
{
    [Collection("AppSpecs")]
    public class CommentCreationModelSpecs
    {
        private readonly TestApplication _myApp;

        public CommentCreationModelSpecs(TestApplication app)
        {
            _myApp = app;
        }



        [Fact]
        public void should_validate_empty_content_as_invalid()
        {
            var comment = new CommentCreationModel();
            
            var modelState = _myApp.ValidateModel(comment);
            
            Assert.False(modelState.IsValid);
        }
    }
}