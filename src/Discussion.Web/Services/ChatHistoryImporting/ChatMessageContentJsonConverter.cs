using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discussion.Web.Services.ChatHistoryImporting
{
    public class ChatMessageContentJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(MessageContent).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = JObject.Load(reader);
            var messageType = (MessageType)(item["_type"].Value<int>());

            switch (messageType)
            {
                case MessageType.Text:
                    return item.ToObject<TextChatMessageContent>();
                case MessageType.Url:
                    return item.ToObject<UrlChatMessageContent>();
                case MessageType.Image:
                case MessageType.Video:
                case MessageType.Attachment:
                    return item.ToObject<FileChatMessageContent>();
                default:
                    throw new NotSupportedException($"不支持导入 {messageType} 类型的聊天消息");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}