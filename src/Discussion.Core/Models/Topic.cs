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

        public int CreatedBy { get; set; }
        public DateTime? LastRepliedAt { get; set; }

        public int ReplyCount { get; set; }
        public int ViewCount { get; set; }

        [ForeignKey("CreatedBy")]
        public User Author { get; set; }
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