using System.Collections.Generic;
using Discussion.Core.Models;

namespace Discussion.Web.ViewModels
{
    public class TopicViewModel
    {
        public TopicViewModel(){}
        
        private TopicViewModel(Topic topic)
        {
            this.Topic = topic;
        }

        public int Id => this.Topic.Id;

        public string Title { get; set; }
        public Topic Topic { get; }

        public List<Reply> Replies { get; set; }

        public static TopicViewModel CreateFrom(Topic topic, List<Reply> replies)
        {
            return new TopicViewModel(topic)
            {
                Replies = replies
            };
        }


        public class NestedClass
        {
            public string Title { get; set; }
        }
    }
}