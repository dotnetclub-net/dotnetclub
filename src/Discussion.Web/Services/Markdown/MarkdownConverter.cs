﻿using System.Collections.Concurrent;
using Markdig;
using Markdig.Extensions.EmphasisExtras;

namespace Discussion.Web.Services.Markdown
{
    public static class MarkdownConverter
    {
        private static readonly ConcurrentDictionary<int, MarkdownPipeline> Pipelines = new ConcurrentDictionary<int, MarkdownPipeline>();
        
        public static string MdToHtml(this string markdown, int maxHeadingLevel = 2)
        {
            var pipeline = GetMarkdownPipeline(maxHeadingLevel);
            return Markdig.Markdown.ToHtml(markdown ?? string.Empty, pipeline);
        }

        private static MarkdownPipeline GetMarkdownPipeline(int maxHeadingLevel)
        {
            return Pipelines.GetOrAdd(maxHeadingLevel, headingLevel =>
            {
                var markdownPipelineBuilder = new MarkdownPipelineBuilder()
                    .DisableHtml()
                    .UsePipeTables()
                    .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
                    .UseAutoLinks();
                markdownPipelineBuilder
                    .Extensions
                    .AddIfNotAlready(new CustomizableHeadingLevelExtension(headingLevel));

                return markdownPipelineBuilder.Build();
            });
        }
    }
}