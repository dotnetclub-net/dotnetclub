using MarkdownSharp;

namespace Discussion.Web.Services
{
    public class MarkdownRenderService
    {
        public string RenderMarkdownAsHtml(string markdownContent)
        {
            // Create new markdown instance
            var markdownConverter = new Markdown { AllowEmptyLinkText = true };
            markdownConverter.AddExtension(new GfmCodeBlocks(markdownConverter));

            return markdownConverter.Transform(markdownContent);
        }
    }
}
