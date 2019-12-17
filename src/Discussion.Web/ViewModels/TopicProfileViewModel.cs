using System;
using Discussion.Core.Models;

namespace Discussion.Web.ViewModels
{
    public class TopicProfileViewModel
    {
        public int Id { get; set; }
        
        public string Title { get; set; }

        public DateTime CreateTime { get; set; }

        public int ViewCount { get; set; }
        
        public int ReplyCount { get; set; }
        
        public TopicType Type { get; set; }
    }
}