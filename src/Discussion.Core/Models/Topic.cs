using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discussion.Core.Models
{
    public class Topic : Entity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public TopicType Type { get; set; }

        [ForeignKey("CreatedBy")]
        public User CreatedByUser { get; set; }
        public int CreatedBy { get; set; }

        [ForeignKey("LastRepliedBy")]
        public User LastRepliedByUser { get; set; }
        public int? LastRepliedBy { get; set; }
        public DateTime? LastRepliedAt { get; set; }

        public int ReplyCount { get; set; }
        public int ViewCount { get; set; }
    }

    public enum TopicType
    {
        [Display(Name = "讨论")]
        Discussion = 1,

        [Display(Name = "问答")]
        Question = 2,

        [Display(Name = "招聘")]
        Job = 4
    }
}