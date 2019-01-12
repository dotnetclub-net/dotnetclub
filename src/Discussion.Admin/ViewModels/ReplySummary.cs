using System;

namespace Discussion.Admin.ViewModels
{
    public class ReplySummary
    {
        public int TopicId { get; set; }
        public string HtmlContent { get; set; }
        public string MarkdownContent { get; set; }

        public UserSummary Author { get; set; }
        public int Id { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}