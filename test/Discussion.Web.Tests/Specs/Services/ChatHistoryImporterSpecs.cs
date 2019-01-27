using System.IO;
using System.Linq;
using System.Text;
using Discussion.Web.Services.ChatHistoryImporting;
using Newtonsoft.Json;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class ChatHistoryImporterSpecs
    {
        [Fact]
        public void should_import_sample_messages()
        {
            const string messageJsonResName = "Discussion.Web.Tests.Fixtures.SampleMessages.json";
            var resourceStream = typeof(ChatHistoryImporterSpecs).Assembly.GetManifestResourceStream(messageJsonResName);

            ChatMessage[] messages;
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                var json = reader.ReadToEnd();
                messages = JsonConvert.DeserializeObject<ChatMessage[]>(json, new MessageContentJsonConverter());
            }
            
            Assert.Equal(8, messages.Length);
            Assert.True(messages.All(msg => msg.Content != null));
            Assert.True(messages.All(msg => msg.SourceName != null));
            Assert.True(messages.All(msg => msg.SourceUserId != null));
        }

    }
}