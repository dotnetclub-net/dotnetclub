using Discussion.Web.Controllers;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using Discussion.Web.Models;
using Xunit;

namespace Discussion.Web.Tests.Specs.Models
{
    [Collection("AppSpecs")]
    public class CreateTopicModelValidationSpecs
    {
        private readonly Application _myApp;
        public CreateTopicModelValidationSpecs(Application app)
        {
            _myApp = app;
        }

        [Fact]
        public void should_validate_normal_title_and_content_values_as_valid()
        {
            var topicController = CreateControllerAndValidateTopic(CreateString(100), CreateString(2000));

            topicController.ModelState.Keys.ShouldNotContain("Title");
            topicController.ModelState.Keys.ShouldNotContain("Content");
        }
        
        [Fact]
        public void should_validate_type_values_as_valid()
        {
            var topicController = CreateControllerAndValidateTopic(CreateString(100), CreateString(2000), (TopicType)0);

            topicController.ModelState.Keys.ShouldContain("Type");
        }

        [Fact]
        public void should_confirm_title_content_type_are_required_on_creating_a_topic()
        {
            var topicController = CreateControllerAndValidateTopic(string.Empty, null, null);

            topicController.ModelState.IsValid.ShouldEqual(false);
            topicController.ModelState.ErrorCount.ShouldEqual(3);

            topicController.ModelState.Keys.ShouldContain("Title");
            topicController.ModelState.Keys.ShouldContain("Content");
            topicController.ModelState.Keys.ShouldContain("Type");
        }

        [Fact]
        public void should_confirm_title_and_content_are_less_than_max_length()
        {
            var topicController = CreateControllerAndValidateTopic(CreateString(255 + 1), CreateString(200 * 1000 + 1));

            topicController.ModelState.IsValid.ShouldEqual(false);
            topicController.ModelState.ErrorCount.ShouldEqual(2);

            topicController.ModelState.Keys.ShouldContain("Title");
            topicController.ModelState.Keys.ShouldContain("Content");
        }


        [Theory]
        [InlineData("</br>")]
        [InlineData("<a")]
        [InlineData("&#30;")]
        public void should_refuse_any_html_tag_in_title_or_content(string illigalFragment)
        {
            var illigalContent = $"html tag like characters {illigalFragment} should not be valid";

            var topicController = CreateControllerAndValidateTopic(illigalContent, illigalContent);

            topicController.ModelState.IsValid.ShouldEqual(false);
            topicController.ModelState.ErrorCount.ShouldEqual(2);

            topicController.ModelState.Keys.ShouldContain("Title");
            topicController.ModelState.Keys.ShouldContain("Content");
        }


        private TopicController CreateControllerAndValidateTopic(string title, string content, TopicType? type = TopicType.Discussion)
        {
            var createModel = new TopicCreationModel { Title = title, Content = content, Type = type};
            return _myApp.CreateControllerAndValidate<TopicController>(createModel);
        }

        private static string CreateString(int length)
        {
            if(length <= 0)
            {
                return string.Empty;
            }

            var letter = Guid.NewGuid().ToString("N")[0];
            return new string(letter, length);
        }
    }
}
