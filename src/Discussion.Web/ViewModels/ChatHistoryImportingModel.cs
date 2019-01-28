namespace Discussion.Web.ViewModels
{
    public class ChatHistoryImportingModel
    {
        public string Title { get; set; }
        public string ChatId { get; set; }
        
        public int[] SelectedIndex { get; set; }
        public bool UseGeneratedNames { get; set; }
        public bool UseGeneratedAvatars { get; set; }
    }
}