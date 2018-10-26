using Discussion.Core.Utilities;
using Discussion.Web.Services.Markdown;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Controllers
{
    [Route("common")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        [HttpPost("md2html")]
        public IActionResult RenderMarkdown([FromForm]string markdown, int maxHeadingLevel = 2)
        {
            var htmlString = string.IsNullOrWhiteSpace(markdown)
                 ? markdown
                 : markdown.MdToHtml(maxHeadingLevel);

            return Ok(ApiResponse.ActionResult(new { html = htmlString }));
        }
    }
}