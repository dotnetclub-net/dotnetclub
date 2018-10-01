using Markdig;
using MarkdownHelper = Markdig.Markdown;

namespace Discussion.Web.Services.Markdown
{
    public static class MarkdownConverter
    {
        public static string ToHtml(string markdown, int maxHeadingLevel = 2)
        {
            var markdownPipelineBuilder = new MarkdownPipelineBuilder()
                .DisableHtml()
                .UseAutoLinks();
            markdownPipelineBuilder
                .Extensions
                .AddIfNotAlready(new CustomizableHeadingLevelExtension(maxHeadingLevel));
            
            var pipeline = markdownPipelineBuilder.Build();
            return MarkdownHelper.ToHtml(markdown ?? string.Empty, pipeline);
        }

    }
}