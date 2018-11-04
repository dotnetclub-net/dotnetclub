namespace Discussion.Admin.ViewModels
{
    public class TopicItemSummary
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public UserSummery Author { get; set; }
    }
    
    public class TopicItem : TopicItemSummary
    {
        public string MarkdownContent { get; set; }
    }
}