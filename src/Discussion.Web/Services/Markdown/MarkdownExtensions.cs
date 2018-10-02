using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Web.Services.Markdown
{
    public static class MarkdownExtensions
    {
        public static string GetContentAsHtml(this string origin)
        {
            return string.IsNullOrWhiteSpace(origin)
                ? origin
                : MarkdownConverter.ToHtml(origin, maxHeadingLevel: 3);
        }
    }
}