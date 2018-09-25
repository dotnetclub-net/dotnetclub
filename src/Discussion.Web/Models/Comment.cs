using Discussion.Web.Data;

namespace Discussion.Web.Models
{
    public class Comment: Entity
    {
        public int TopicId { get; set; }
        public int CreatedBy { get; set; }
        public string Content { get; set; }
    }
}