using Newtonsoft.Json;

namespace Discussion.Web.Services.ChatHistoryImporting
{
    public class ChatMessage
    {
        [JsonProperty("_sourceName")]
        public string SourceName {get;set;}

        [JsonProperty("_sourceUserId")]
        public string SourceUserId {get;set;}

        [JsonProperty("_sourceTime")]
        public string SourceTime {get;set;}

        [JsonProperty("_sourceTimestamp")]
        public string SourceTimestamp {get;set;}

        [JsonProperty("_content")]
        public MessageContent Content {get;set;}
    }
    
    public abstract class MessageContent
    {
        [JsonProperty("_type")]
        public MessageType Type { get; set; }
    }

    public class TextChatMessageContent : MessageContent
    {
        [JsonProperty("_text")]
        public string Text { get; set; }
    }

    public class UrlChatMessageContent : MessageContent
    {
        [JsonProperty("_link")]
        public string Link { get; set; }
        
        [JsonProperty("_title")]
        public string Title { get; set; }
        
        [JsonProperty("_description")]
        public string Description { get; set; }
    }

    public class FileChatMessageContent : MessageContent
    {
        [JsonProperty("_fileId")]
        public string FileId { get; set; }
        
        [JsonProperty("_fileName")]
        public string FileName { get; set; }
    }
    
    public enum MessageType {
        Unknown = 0,
        Text = 1,
    
        Image = 2,
        Video = 4,
        Url = 5,
        Attachment = 8,
    
        ChatHistory = 17,

        // TinyVideo = 888,
    }
}