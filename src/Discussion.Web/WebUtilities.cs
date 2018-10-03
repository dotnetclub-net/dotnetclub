using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Discussion.Core.Models;
using Discussion.Web.Services.Markdown;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Discussion.Web
{
    public static class WebUtilities
    {
        public static IHtmlContent Timestamp(this IHtmlHelper html, DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return html.Raw(string.Empty);
            }

            var time = dateTime.Value;
            var utcDateTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Utc);
            var timestamp = new DateTimeOffset(utcDateTime).ToUnixTimeMilliseconds();
            return html.Raw(timestamp.ToString());
        }

        public static IHtmlContent DescribeTopicType(this IHtmlHelper html, TopicType topicType)
        {
            var memberInfo = typeof(TopicType).GetMember(topicType.ToString()).FirstOrDefault();
            return new HtmlString(memberInfo.GetCustomAttribute<DisplayAttribute>().Name);
        }
        
        public static IHtmlContent RenderMarkdown(this IHtmlHelper html, string markdown, int maxHeadingLevel = 2)
        {
            var htmlString = string.IsNullOrWhiteSpace(markdown)
                ? markdown
                : markdown.MdToHtml(maxHeadingLevel);

            return html.Raw(htmlString);
        }
    }
}