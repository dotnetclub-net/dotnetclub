using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs
{
    public class WebUtilitiesSpecs
    {
        [Fact]
        public void should_extract_anti_forgery_token_value()
        {
            var token = "CfDJ8Nu_U0HhzZlHpzrWu2WpGhHE9IMyHjVvuRLL-16gLzX8MfBISuvyoUoWUQGV2FpoWmKvw4vHnndpah3qZVVNeTCZWw3KPSFXt-GOldZ6YxGAsFwWjoyYZQrgYX62lsLAMlTHxB6dMSkJZubXMfAY4mNe24mffRgATfsQomRNl7NVmIevKwvzje7sxbLA9Veb2w";
            var tokenInputTag = $@"<input name=""__RequestVerificationToken"" type=""hidden"" value=""{token}"" />";
            var htmlHelper = new Mock<IHtmlHelper>();
            htmlHelper.Setup(helper => helper.AntiForgeryToken())
                      .Returns(new HtmlString(tokenInputTag));

            var tokenValue = htmlHelper.Object.AntiForgeryTokenValue();
            Assert.Equal(token, tokenValue.ToString());
        }
    }
}