using System.ComponentModel.DataAnnotations.Schema;

namespace Discussion.Core.Models
{
    public class Reply : Entity
    {
        public int TopicId { get; set; }

        [ForeignKey("CreatedBy")]
        public User CreatedByUser { get; set; }
        public int CreatedBy { get; set; }
        
        public string Content { get; set; }

        public IAuthor Author { get; set; }
    }
}
