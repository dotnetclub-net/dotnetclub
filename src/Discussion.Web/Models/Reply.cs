using System.ComponentModel.DataAnnotations.Schema;
using Discussion.Web.Data;
using Discussion.Web.Services.Markdown;

namespace Discussion.Web.Models
{
    public class Reply: Entity
    {
        public int TopicId { get; set; }
        public int CreatedBy { get; set; }
        public string Content { get; set; }
        
        [ForeignKey("CreatedBy")]
        public User Author { get; set; }

        public string GetContentAsHtml()
        {
            return string.IsNullOrWhiteSpace(Content)
                ? Content
                : MarkdownConverter.ToHtml(Content, maxHeadingLevel: 3);
        }
    }
}