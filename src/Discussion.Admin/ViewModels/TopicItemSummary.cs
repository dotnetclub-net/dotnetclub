namespace Discussion.Admin.ViewModels
{
    public class TopicItemSummary
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public AuthorSummary Author { get; set; }
        public int ViewCount { get; set; }
        public int ReplyCount { get; set; }
    }
    
    public class TopicItem : TopicItemSummary
    {
        public string MarkdownContent { get; set; }
        public string HtmlContent { get; set; }
    }
}