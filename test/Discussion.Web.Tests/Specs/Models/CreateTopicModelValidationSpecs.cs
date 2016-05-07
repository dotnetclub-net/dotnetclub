using Discussion.Web.Controllers;
using Discussion.Web.ViewModels;
using Xunit;

namespace Discussion.Web.Tests.Specs.Models
{
    [Collection("AppSpecs")]
    public class CreateTopicModelValidationSpecs
    {
        public Application _myApp;
        public CreateTopicModelValidationSpecs(Application app)
        {
            _myApp = app;
        }

        [Fact]
        public void should_confirm_title_and_content_are_required_on_creating_a_topic()
        {
            var title = string.Empty;
            var content = string.Empty;
            var createModel = new TopicCreationModel { Title = title, Content = content };

            var topicController = _myApp.CreateController<TopicController>();
            topicController.TryValidateModel(createModel);


            topicController.ModelState.IsValid.ShouldEqual(false);
            topicController.ModelState.ErrorCount.ShouldEqual(2);

            topicController.ModelState.Keys.ShouldContain("Title");
            topicController.ModelState.Keys.ShouldContain("Content");
        }
    }
}
