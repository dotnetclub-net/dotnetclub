using System.ComponentModel.DataAnnotations.Schema;

namespace Discussion.Core.Models
{
    public class Reply : Entity
    {
        public int TopicId { get; set; }
        public int CreatedBy { get; set; }
        public string Content { get; set; }

        [ForeignKey("CreatedBy")]
        public User Author { get; set; }
    }
}