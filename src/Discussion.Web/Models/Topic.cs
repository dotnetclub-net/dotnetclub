using Discussion.Web.Data;
using System;

namespace Discussion.Web.Models
{
    public class Topic : Entity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public TopicType TopicType { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime? LastRepliedAt { get; set; }

        public int ReplyCount { get; set; }
        public int ViewCount { get; set; }

    }



    public enum TopicType
    {
        Sharing = 1,

        Question = 2
    }
}
