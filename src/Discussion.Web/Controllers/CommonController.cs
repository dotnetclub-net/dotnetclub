using Discussion.Web.Services.Markdown;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Controllers
{
    [Route("api/common")]
    public class CommonController : ControllerBase
    {
        [HttpPost("md2html")]
        public object RenderMarkdown([FromForm]string markdown, int maxHeadingLevel = 2)
        {
            var htmlString = string.IsNullOrWhiteSpace(markdown)
                 ? markdown
                 : markdown.MdToHtml(maxHeadingLevel);

            return new { html = htmlString };
        }
    }
}