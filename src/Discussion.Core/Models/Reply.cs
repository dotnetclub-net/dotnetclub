using System.ComponentModel.DataAnnotations.Schema;

namespace Discussion.Core.Models
{
    public class Reply : Entity
    {
        public int TopicId { get; set; }

        [ForeignKey("CreatedBy")]
        public User CreatedByUser { get; set; }
        public int? CreatedBy { get; set; }
        
        [ForeignKey("CreatedByWeChat")]
        public WeChatAccount CreatedByWeChatAccount { get; set; }
        public int? CreatedByWeChat { get; set; }
        
        public string Content { get; set; }

        [NotMapped] public IAuthor Author => (IAuthor)this.CreatedByUser ?? this.CreatedByWeChatAccount;
    }
}
