using System.Collections.Generic;
using Discussion.Core.Models;

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

        public List<Reply> Replies { get; set; }

        public static TopicViewModel CreateFrom(Topic topic, List<Reply> replies)
        {
            return new TopicViewModel(topic)
            {
                Replies = replies
            };
        }
    }
}