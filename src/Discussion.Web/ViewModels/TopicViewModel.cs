using System;
using System.Collections.Generic;
using Discussion.Web.Models;
using Discussion.Web.Services.Markdown;

namespace Discussion.Web.ViewModels
{
    public class TopicViewModel
    {
        private TopicViewModel(Topic topic)
        {
            this.Topic = topic;
        }
        
        public int Id => this.Topic.Id;

        public Topic Topic { get; }
        
        public string HtmlContent { get; set; }
        
        public List<Reply> Replies { get; set; }
        


        public static TopicViewModel CreateFrom(Topic topic, List<Reply> replies)
        {
            return new TopicViewModel(topic)
            {
                HtmlContent = MarkdownConverter.ToHtml(topic.Content),
                Replies = replies
            };
        }
    }

}
