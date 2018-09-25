using System.Collections.Generic;
using Discussion.Web.Models;
using Discussion.Web.Services.Markdown;

namespace Discussion.Web.ViewModels
{
    public class TopicViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string MarkdownContent { get; set; }

        public string HtmlContent { get; set; }
        public List<Comment> Comments { get; set; }
        
        public int ReplyCount { get; set; }
        public int ViewCount { get; set; }



        public static TopicViewModel CreateFrom(Topic topic, List<Comment> comments)
        {
            return new TopicViewModel
            {
                Id = topic.Id,
                Title = topic.Title,
                MarkdownContent = topic.Content,
                ReplyCount = topic.ReplyCount,
                ViewCount = topic.ViewCount,
                HtmlContent = MarkdownConverter.ToHtml(topic.Content),
                Comments = comments
            };
        }
    }

}
