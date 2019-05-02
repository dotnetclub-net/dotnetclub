using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.FileSystem;
using Discussion.Core.Models;
using Discussion.Core.Time;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Services.ChatHistoryImporting
{
    public class DefaultChatHistoryImporter : IChatHistoryImporter
    {
        private readonly IClock _clock;
        private readonly IUrlHelper _urlHelper;
        private readonly IRepository<FileRecord> _fileRepo;
        private readonly IRepository<WeChatAccount> _weChatAccountRepo;
        private readonly IFileSystem _fileSystem;
        private readonly ICurrentUser _currentUser;
        private readonly ChatyApiService _chatyApiService;

        public DefaultChatHistoryImporter(IClock clock,
            IUrlHelper urlHelper,
            IRepository<FileRecord> fileRepo,
            IRepository<WeChatAccount> weChatAccountRepo,
            IFileSystem fileSystem,
            ICurrentUser currentUser, ChatyApiService chatyApiService)
        {
            _clock = clock;
            _urlHelper = urlHelper;
            _fileRepo = fileRepo;
            _weChatAccountRepo = weChatAccountRepo;
            _fileSystem = fileSystem;
            _currentUser = currentUser;
            _chatyApiService = chatyApiService;
        }

        public async Task<List<Reply>> Import(ChatMessage[] wechatMessages)
        {
            if (wechatMessages == null || wechatMessages.Length < 1)
            {
                throw new ArgumentException("微信消息为空，无法导入", nameof(wechatMessages));
            }

            var replies = new List<Reply>();
            for (var i = 0; i < wechatMessages.Length; i++)
            {
                var (content, idx) = await ImportContentWithMerge(wechatMessages, i);
                var chatMessage = wechatMessages[i];
                var wechatAccount = GetOrCreateGetWeChatAccount(chatMessage);
                
                replies.Add(new Reply
                {
                    Content = content,
                    CreatedAtUtc = chatMessage.SourceTimestamp <= 0 
                        ? _clock.Now.UtcDateTime : 
                        DateTimeOffset.FromUnixTimeSeconds(chatMessage.SourceTimestamp).UtcDateTime,
                    CreatedByWeChatAccount = wechatAccount
                });

                i = idx;
            }
            
            return replies;
        }

        private async Task<(string, int)> ImportContentWithMerge(ChatMessage[] wechatMessages, int curIndex)
        {
            var curMessage = wechatMessages[curIndex];
            
            var content = await GetContentFromMessage(curMessage);
            var contentBuilder = new StringBuilder(content);

            if (!IsTextMessage(curMessage))
            {
                return (contentBuilder.ToString(), curIndex);
            }

            while (curIndex + 1 < wechatMessages.Length)
            {
                var nextMessage = wechatMessages[curIndex + 1];
                var shouldMerge = nextMessage != null
                                  && nextMessage.SourceWxId == curMessage.SourceWxId
                                  && IsTextMessage(nextMessage);
                if (!shouldMerge)
                {
                    break;
                }

                var nextContent = await GetContentFromMessage(nextMessage);
                contentBuilder.Append("\n\n");
                contentBuilder.Append(nextContent);
                curIndex++;
            }

            return (contentBuilder.ToString(), curIndex);
        }

        private static bool IsTextMessage(ChatMessage chatMessage)
        {
            return chatMessage.Content.Type == MessageType.Text
                   || chatMessage.Content.Type == MessageType.Url
                   || chatMessage.Content.Type == MessageType.Attachment;
        }

        private WeChatAccount GetOrCreateGetWeChatAccount(ChatMessage msg)
        {
            var wxId = msg.SourceWxId;
            var wechatAccount = _weChatAccountRepo.All().FirstOrDefault(wxa => wxa.WxId.ToLower() == wxId.ToLower());
            if (wechatAccount == null)
            {
                wechatAccount = new WeChatAccount
                {
                    WxId = msg.SourceWxId
                };
                _weChatAccountRepo.Save(wechatAccount);
            }

            return wechatAccount;
        }


        private async Task<string> GetContentFromMessage(ChatMessage msg)
        {
            var msgContent = msg.Content;
            switch (msgContent.Type)
            {
                case MessageType.Text:
                    return (msgContent as TextChatMessageContent)?.Text;
                case MessageType.Url:
                    return GetUrlContent(msgContent as UrlChatMessageContent);
                case MessageType.Image:
                    return await GetImageContent(msgContent as FileChatMessageContent);
                case MessageType.Attachment:
                    return await GetFileContent(msgContent as FileChatMessageContent);
                case MessageType.Video:
                    return "[视频]";
                default:
                    throw new NotSupportedException($"不支持导入 {msgContent.Type} 类型的聊天消息");
            }
        }

        private async Task<string> GetImageContent(FileChatMessageContent fileChatMessageContent)
        {
            var url = await FetchToLocal(fileChatMessageContent.FileName, fileChatMessageContent.FileId);
            return $"![{fileChatMessageContent.FileName}]({url}#middle)";
        }
        
        private async Task<string> GetFileContent(FileChatMessageContent msgContent)
        {
            var url = await FetchToLocal(msgContent.FileName, msgContent.FileId);
            return $"[下载文件 {msgContent.FileName}]({url})";
        }

        private string GetUrlContent(UrlChatMessageContent msgContent)
        {
            return $"[{msgContent.Title}]({msgContent.Link})";
        }

        private async Task<string> FetchToLocal(string fileName, string fileId)
        {
            var downloadedStream = await _chatyApiService.DownloadChatFile(fileId, fileName);
            return await SaveFile(fileName, downloadedStream);
        }

        private async Task<string> SaveFile(string name, Stream content)
        {
            const string category = "imported-reply";
            var subPath = string.Concat(category, _fileSystem.GetDirectorySeparatorChar(),
                Guid.NewGuid().ToString("N"));

            var storageFile = await _fileSystem.CreateFileAsync(subPath);
            using (var outputStream = await storageFile.OpenWriteAsync())
            {
                await content.CopyToAsync(outputStream);
            }

            var fileRecord = new FileRecord
            {
                UploadedBy = _currentUser.DiscussionUser.Id,
                Size = content.Length,
                OriginalName = name,
                StoragePath = subPath,
                Category = category,
                Slug = Guid.NewGuid().ToString("N")
            };
            _fileRepo.Save(fileRecord);

            // ReSharper disable once Mvc.ActionNotResolved
            // ReSharper disable once Mvc.ControllerNotResolved
            return _urlHelper.Action("DownloadFile", "Common", new {slug = fileRecord.Slug});
        }
    }
}