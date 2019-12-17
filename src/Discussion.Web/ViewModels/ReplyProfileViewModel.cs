using System;

namespace Discussion.Web.ViewModels
{
    public class ReplyProfileViewModel
    {
        public int TopicId { get; set; }
        
        public string TopicName { get; set; }
        
        public string ReplyContent { get; set; }
        
        public DateTime ReplyCreateTime { get; set; }
    }
}