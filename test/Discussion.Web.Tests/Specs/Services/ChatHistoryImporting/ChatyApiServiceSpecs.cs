using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discussion.Core.Utilities;
using Discussion.Web.Services.ChatHistoryImporting;
using Discussion.Web.Tests.Stubs;
using Discussion.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services.ChatHistoryImporting
{
    public class ChatyApiServiceSpecs
    {
        private readonly StubHttpClient _httpClient;
        private readonly ChatyApiService _normalChatyApiService;
        private readonly string _userAllowedToUseChaty = "user_in_list";

        public ChatyApiServiceSpecs()
        {
            _httpClient = StubHttpClient.Create()
                .Json("/chat/list/some_wx_id", new[] {"1556867244", "1556867241"})
                .Json("/chat/detail/some_wx_id/1556867244", new[] {
                    new ChatMessage
                    {
                        SourceName = "someone",
                        SourceTime = "2018/12/02 12:00:08",
                        SourceTimestamp = 1547552931,
                        SourceWxId = "Wx_879LJJKJGS",
                        Content = new TextChatMessageContent()
                        {
                            Text = "text1"
                        }
                    },
                    new ChatMessage
                    {
                        SourceName = "Another one",
                        SourceTime = "2018/12/02 12:00:09",
                        SourceTimestamp = 1547552932,
                        SourceWxId = "Wx_879LKJGSJJ",
                        Content = new TextChatMessageContent()
                        {
                            Text = "text2"
                        }
                    }})
                .Json("/chat/detail/some_wx_id/1556867241", new[] {
                    new ChatMessage
                    {
                        SourceName = "someone",
                        SourceTime = "2018/12/02 12:00:08",
                        SourceTimestamp = 1547552933,
                        SourceWxId = "Wx_879LJJKJGS",
                        Content = new FileChatMessageContent()
                        {
                            FileId = "89257293",
                            FileName = "SomeFile.jpg"
                        }
                    },
                    new ChatMessage
                    {
                        SourceName = "Another one",
                        SourceTime = "2018/12/02 12:00:09",
                        SourceTimestamp = 1547552934,
                        SourceWxId = "Wx_879LKJGSJJ",
                        Content = new UrlChatMessageContent()
                        {
                            Link = "http://dotnetclub"
                        }
                    }})
                .Json("/bot/info", new ChatyBotInfoViewModel
                {
                    Name = "Someone", 
                    QrCode = "https://weixin.com/2o3kl3z", 
                    Weixin = "wx_l09d",
                    ChatyId = "34kds"
                }).Json("/bot/pair", new ChatyVerifyResultViewModel
                {
                    Name = "Someone",  
                    Id = "wx_l09d"
                }).When(req =>
                {
                    if (req.RequestUri.PathAndQuery.StartsWith("/chat/file/"))
                    {
                        var ms = new MemoryStream();
                        ms.Write(Encoding.UTF8.GetBytes("This is file content"));
                        ms.Seek(0, SeekOrigin.Begin);

                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StreamContent(ms)
                            {
                                Headers =
                                {
                                    ContentType = new MediaTypeHeaderValue("application/octet-stream")
                                }
                            }
                        };
                    }

                    return null;
                });
            
            var chatyOptions = new Mock<IOptions<ChatyOptions>>();
            chatyOptions.SetupGet(op => op.Value).Returns(new ChatyOptions
            {
                ServiceBaseUrl = "http://localhost:3000",
                AllowedUsers = string.Format("{0},someone_else", _userAllowedToUseChaty)
            });
            _normalChatyApiService = new ChatyApiService(chatyOptions.Object, _httpClient, NullLogger<ChatyApiService>.Instance);
        }

        [Theory]
        [MemberData(nameof(GetChatyNotSupportedScenarios))]
        public void should_check_support_as_false(ChatyApiService service)
        {
            Assert.False(service.IsChatySupported(_userAllowedToUseChaty));
        }
        
        [Fact]
        public void should_check_support_as_false_when_user_not_in_list()
        {
            Assert.False(_normalChatyApiService.IsChatySupported("user_not_in_list"));
        }
        
        [Fact]
        public void should_check_support_as_true_when_user_in_list()
        {
            Assert.True(_normalChatyApiService.IsChatySupported(_userAllowedToUseChaty));
            Assert.Equal(0, _httpClient.RequestsSent.Count);
        }
        
        [Fact]
        public void should_check_support_as_true_when_allowed_users_not_configured()
        {
            var optionsWithoutAllowedUsers = new Mock<IOptions<ChatyOptions>>();
            optionsWithoutAllowedUsers.SetupGet(op => op.Value).Returns(new ChatyOptions()
            {
                ServiceBaseUrl = "http://abcd",
                AllowedUsers = null
            });

            var chatyApiService = new ChatyApiService(optionsWithoutAllowedUsers.Object, null, null);

            Assert.True(chatyApiService.IsChatySupported(StringUtility.Random(6)));
        }
        
        public static IEnumerable<object[]> GetChatyNotSupportedScenarios()
        {
            var nullOptions = new Mock<IOptions<ChatyOptions>>();
            nullOptions.SetupGet(op => op.Value).Returns((ChatyOptions) null);
            
            var emptyServiceUrlOptions = new Mock<IOptions<ChatyOptions>>();
            emptyServiceUrlOptions.SetupGet(op => op.Value).Returns(new ChatyOptions());
            
            var allData = new List<object[]>
            {
                new object[]{ new ChatyApiService(null, null, null) },
                new object[]{ new ChatyApiService(nullOptions.Object, null, null) },
                new object[]{ new ChatyApiService(emptyServiceUrlOptions.Object, null, null) },
            };

            return allData;
        }

        [Fact]
        public async Task should_get_message_list()
        { 
            var messageList = await _normalChatyApiService.GetMessageList("some_wx_id");
            
            Assert.Equal(3, _httpClient.RequestsSent.Count);
            Assert.Contains(_httpClient.RequestsSent, req => req.RequestUri.PathAndQuery == "/chat/list/some_wx_id");
            Assert.Contains(_httpClient.RequestsSent, req => req.RequestUri.PathAndQuery == "/chat/detail/some_wx_id/1556867244");
            Assert.Contains(_httpClient.RequestsSent, req => req.RequestUri.PathAndQuery == "/chat/detail/some_wx_id/1556867241");
            
            Assert.NotNull(messageList);
            Assert.Equal(2, messageList.Count);
            
            var firstMsg = messageList[0];
            Assert.Equal("1556867244", firstMsg.ChatId);
            Assert.Equal("text1...,text2...", string.Join(",", firstMsg.MessageSummaryList));
            
            var secondMsg = messageList[1];
            Assert.Equal("1556867241", secondMsg.ChatId);
            Assert.Equal("[文件],[链接]", string.Join(",", secondMsg.MessageSummaryList));
        }
        
        [Fact]
        public async Task should_get_messages_in_chat()
        { 
            var messages = await _normalChatyApiService.GetMessagesInChat("some_wx_id", "1556867241");
            
            Assert.Equal(1, _httpClient.RequestsSent.Count);
            Assert.Contains(_httpClient.RequestsSent, req => req.RequestUri.PathAndQuery == "/chat/detail/some_wx_id/1556867241");
            
            Assert.NotNull(messages);
            Assert.Equal(2, messages.Length);
            
            var firstMsg = messages[0];
            Assert.Equal("Wx_879LJJKJGS", firstMsg.SourceWxId);
            Assert.Equal(1547552933, firstMsg.SourceTimestamp);
            
            var fileMessage = firstMsg.Content as FileChatMessageContent;
            Assert.NotNull(fileMessage);
            Assert.Equal("89257293", fileMessage.FileId);
            Assert.Equal("SomeFile.jpg", fileMessage.FileName);
            
            var secondMsg = messages[1];
            Assert.Equal("Wx_879LKJGSJJ", secondMsg.SourceWxId);
            Assert.Equal(1547552934, secondMsg.SourceTimestamp);
            
            var urlMessage = secondMsg.Content as UrlChatMessageContent;
            Assert.NotNull(urlMessage);
            Assert.Equal("http://dotnetclub", urlMessage.Link);
        }

        [Fact]
        public async Task should_get_bot_status()
        {
            var botStatus = await _normalChatyApiService.GetChatyBotStatus();
            
            Assert.Equal(1, _httpClient.RequestsSent.Count);
            Assert.Contains(_httpClient.RequestsSent, req => req.RequestUri.PathAndQuery == "/bot/info");
            
            Assert.NotNull(botStatus);
            Assert.Equal("https://weixin.com/2o3kl3z", botStatus.QrCode);
            Assert.Equal("Someone", botStatus.Name);
        }
        
        
        [Fact]
        public async Task should_verify_wechat_account()
        {
            var verifyResult = await _normalChatyApiService.VerifyWeChatAccount("839425");
            
            Assert.Equal(1, _httpClient.RequestsSent.Count);
            Assert.Contains(_httpClient.RequestsSent, req => req.Method == HttpMethod.Post && req.RequestUri.PathAndQuery == "/bot/pair");
            
            Assert.NotNull(verifyResult);
            Assert.Equal("wx_l09d", verifyResult.Id);
        }       
        
        [Fact]
        public async Task should_download_file_from_chaty()
        {
            var downloadResult = await _normalChatyApiService.DownloadChatFile("839425", "filename.txt");
            
            Assert.Equal(1, _httpClient.RequestsSent.Count);
            Assert.Contains(_httpClient.RequestsSent, req => req.RequestUri.PathAndQuery == "/chat/file/839425");
            
            Assert.NotNull(downloadResult);
        }
        
        

    }
}