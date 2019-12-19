using System;
using Discussion.Core.Models;

namespace Discussion.Web.ViewModels
{
    public class ReplyProfileViewModel
    {
        public User CreatedByUser { get; set; }
        public int TopicId { get; set; }
        
        public string ReplyContent { get; set; }
        
        public DateTime ReplyCreateTime { get; set; }
    }
}