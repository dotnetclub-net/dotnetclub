using Discussion.Web.Controllers;
using Discussion.Web.Models;
using Discussion.Web.ViewModels;
using Xunit;

namespace Discussion.Web.Tests.Specs.Models
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
            
            var controller = _myApp.CreateControllerAndValidate<CommentController>(comment);
            
            Assert.False(controller.ModelState.IsValid);
        }
    }
}