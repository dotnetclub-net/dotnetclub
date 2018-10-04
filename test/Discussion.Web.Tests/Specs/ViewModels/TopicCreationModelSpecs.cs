using System;
using Discussion.Core.Models;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace Discussion.Web.Tests.Specs.ViewModels
{
    [Collection("AppSpecs")]
    public class CreateTopicModelSpecs
    {
        private readonly TestApplication _myApp;

        public CreateTopicModelSpecs(TestApplication app)
        {
            _myApp = app;
        }

        [Fact]
        public void should_validate_normal_title_and_content_values_as_valid()
        {
            var modelState = ValidateTopic(CreateString(100), CreateString(2000));

            modelState.Keys.ShouldNotContain("Title");
            modelState.Keys.ShouldNotContain("Content");
        }

        [Fact]
        public void should_validate_type_values_as_valid()
        {
            var modelState = ValidateTopic(CreateString(100), CreateString(2000), (TopicType)0);

            modelState.Keys.ShouldContain("Type");
        }

        [Fact]
        public void should_confirm_title_content_type_are_required_on_creating_a_topic()
        {
            var modelState = ValidateTopic(string.Empty, null, null);

            modelState.IsValid.ShouldEqual(false);
            modelState.ErrorCount.ShouldEqual(3);

            modelState.Keys.ShouldContain("Title");
            modelState.Keys.ShouldContain("Content");
            modelState.Keys.ShouldContain("Type");
        }

        [Fact]
        public void should_confirm_title_and_content_are_less_than_max_length()
        {
            var modelState = ValidateTopic(CreateString(255 + 1), CreateString(200 * 1000 + 1));

            modelState.IsValid.ShouldEqual(false);
            modelState.ErrorCount.ShouldEqual(2);

            modelState.Keys.ShouldContain("Title");
            modelState.Keys.ShouldContain("Content");
        }

        [Theory]
        [InlineData("</br>")]
        [InlineData("<a")]
        [InlineData("&#30;")]
        public void should_refuse_any_html_tag_in_only_title_not_in_content(string illigalFragment)
        {
            var illigalContent = $"html tag like characters {illigalFragment} should not be valid";

            var modelState = ValidateTopic(illigalContent, illigalContent);

            modelState.IsValid.ShouldEqual(false);
            modelState.ErrorCount.ShouldEqual(1);

            modelState.Keys.ShouldContain("Title");
            modelState.Keys.ShouldNotContain("Content");
        }

        private ModelStateDictionary ValidateTopic(string title, string content, TopicType? type = TopicType.Discussion)
        {
            var createModel = new TopicCreationModel { Title = title, Content = content, Type = type };
            return _myApp.ValidateModel(createModel);
        }

        private static string CreateString(int length)
        {
            if (length <= 0)
            {
                return string.Empty;
            }

            var letter = Guid.NewGuid().ToString("N")[0];
            return new string(letter, length);
        }
    }
}