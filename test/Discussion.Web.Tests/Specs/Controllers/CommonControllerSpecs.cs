using System;
using System.Collections.Generic;
using System.Text;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class CommonControllerSpecs
    {
        private TestDiscussionWebApp _app;

        public CommonControllerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public void convert_markdown_to_html()
        {
            // Act
            var commonController = _app.CreateController<CommonController>();
            var renderResult = commonController.RenderMarkdown("# Title");

            // Assert
            var statusCodeResult = renderResult as OkObjectResult;
            Assert.NotNull(statusCodeResult);
            Assert.Equal(200, statusCodeResult.StatusCode);

            var result = statusCodeResult.Value as ApiResponse;
            Assert.NotNull(result);
        }
    }
}