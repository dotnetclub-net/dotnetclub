using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.FileSystem;
using Discussion.Core.Models;
using Discussion.Core.Time;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Discussion.Web.Services.ChatHistoryImporting
{
    public class DefaultChatHistoryImporter : IChatHistoryImporter
    {
        private readonly IClock _clock;
        private readonly HttpMessageInvoker _httpClient;
        private readonly IUrlHelper _urlHelper;
        private readonly IRepository<FileRecord> _fileRepo;
        private readonly IRepository<WeChatAccount> _weChatAccountRepo;
        private readonly IFileSystem _fileSystem;
        private readonly ICurrentUser _currentUser;
        private readonly ChatyOptions _chatyOptions;

        public DefaultChatHistoryImporter(IClock clock,
            HttpMessageInvoker httpClient,
            IUrlHelper urlHelper,
            IRepository<FileRecord> fileRepo,
            IRepository<WeChatAccount> weChatAccountRepo,
            IFileSystem fileSystem,
            ICurrentUser currentUser, IOptions<ChatyOptions> chatyOptions)
        {
            _clock = clock;
            _httpClient = httpClient;
            _urlHelper = urlHelper;
            _fileRepo = fileRepo;
            _weChatAccountRepo = weChatAccountRepo;
            _fileSystem = fileSystem;
            _currentUser = currentUser;
            _chatyOptions = chatyOptions.Value;
        }

        public async Task<List<Reply>> Import(ChatMessage[] wechatMessages)
        {
            if (wechatMessages == null || wechatMessages.Length < 1)
            {
                throw new ArgumentException("微信消息为空，无法导入", nameof(wechatMessages));
            }

            var convertAll = wechatMessages
                .Select(async msg => new Reply
                {
                    Content = await GetContentFromMessage(msg),
                    CreatedAtUtc = _clock.Now.UtcDateTime,
                    CreatedByWeChatAccount = GetOrCreateGetWeChatAccount(msg)
                })
                .ToArray();
                
            await Task.WhenAll(convertAll);
            
            var replies = convertAll.Select(t => t.Result).ToList();
            return replies;
        }

        private WeChatAccount GetOrCreateGetWeChatAccount(ChatMessage msg)
        {
            var wxId = msg.SourceWxId;
            var wechatAccount = _weChatAccountRepo.All().FirstOrDefault(wxa => wxa.WxId.ToLower() == wxId.ToLower());
            if (wechatAccount == null)
            {
                // todo: real wechat account!!
                // todo: download avatar!!
                wechatAccount = new WeChatAccount
                {
                    WxId = msg.SourceWxId,
                    NickName = msg.SourceName
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
            var serviceBaseUrl = _chatyOptions.ServiceBaseUrl.TrimEnd('/');
            var apiPath = $"{serviceBaseUrl}/chat/file/{fileId}";
            
            var downloadRequest = new HttpRequestMessage();
            downloadRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", _chatyOptions.ApiToken);
            downloadRequest.Method = HttpMethod.Get;
            downloadRequest.RequestUri = new Uri(apiPath);

            var response = await _httpClient.SendAsync(downloadRequest, CancellationToken.None);
            response.EnsureSuccessStatusCode();

            if (!(response.Content is StreamContent file))
            {
                throw new InvalidOperationException($"无法下载文件 {fileName} (id: {fileId})");
            }
            var downloadedStream = await file.ReadAsStreamAsync();
            return await SaveFile(fileName, downloadedStream);
        }

        private async Task<string> SaveFile(string name, Stream content)
        {
            const string category = "imported-reply";
            var subPath = string.Concat(category, _fileSystem.GetDirectorySeparatorChar(), Guid.NewGuid().ToString("N"));
            
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