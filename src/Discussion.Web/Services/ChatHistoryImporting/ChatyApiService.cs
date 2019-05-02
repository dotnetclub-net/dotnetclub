using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Web.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Discussion.Web.Services.ChatHistoryImporting
{
    public class ChatyApiService
    {
        private readonly ChatyOptions _chatyOptions;
        private readonly HttpMessageInvoker _httpClient;
        private readonly ILogger<ChatyApiService> _logger;

        public ChatyApiService(IOptions<ChatyOptions> chatyOptions, HttpMessageInvoker httpClient, ILogger<ChatyApiService> logger)
        {
            _chatyOptions = chatyOptions?.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ChatMessage[]> GetMessagesInChat(string wxId, string chatId)
        {
            var responseString = await InvokeChatyStringApi($"/chat/detail/{wxId}/{chatId}", HttpMethod.Get, null, "调取对话中的消息");
            return responseString == null 
                ? null 
                : JsonConvert.DeserializeObject<ChatMessage[]>(responseString, new ChatMessageContentJsonConverter());
        }

        public async Task<List<ChatyMessageListItemViewModel>> GetMessageList(string wxId)
        {
            var responseString =  await InvokeChatyStringApi($"/chat/list/{wxId}", HttpMethod.Get, null, "导入对话");
            if (responseString == null)
            {
                return null;
            }
            
            var messages = JsonConvert.DeserializeObject<string[]>(responseString);
            var timestampList = messages.Select(Int64.Parse).OrderByDescending(x => x).Take(6).Select(x => x.ToString()).ToList();
            var messageList = new List<ChatyMessageListItemViewModel>();
            foreach (var timestampChatId in timestampList)
            {
                var chatyMessages = await GetMessagesInChat(wxId, timestampChatId);
                var summaryList = chatyMessages.Take(5).Select(msg => msg.Content.Summary).ToArray();
                
                messageList.Add(new ChatyMessageListItemViewModel
                {
                    ChatId = timestampChatId, 
                    MessageSummaryList = summaryList
                });
            }

            return messageList;
        }

        public async Task<string> GetChatyBotStatus()
        {
            return await InvokeChatyStringApi("/bot/info", HttpMethod.Get, null, "获取 Chaty 状态");
        }
        
        public async Task<ChatyVerifyResultViewModel> VerifyWeChatAccount(string code)
        {
            var payload = new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("code", code)});
            var responseString = await InvokeChatyStringApi("/bot/pair", HttpMethod.Post, payload, "验证配对验证码");
            
            return responseString == null ? null : JsonConvert.DeserializeObject<ChatyVerifyResultViewModel>(responseString);
        }

        public async Task<Stream> DownloadChatFile(string fileId, string fileName)
        {
            var apiPath = $"/chat/file/{fileId}";
            var apiUseCase = $"下载文件 {fileName} (id: {fileId})";
            var apiInvocation = await Invoke(apiPath, HttpMethod.Get, null, apiUseCase);

            var response = apiInvocation.Item1;
            if (response == null)
            {
                throw new InvalidOperationException($"无法完成 {apiUseCase}");
            }
            
            var responseStream = apiInvocation.Item2;
            if (!response.IsSuccessStatusCode)
            {
                string responseString;
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    responseString = reader.ReadToEnd();
                }

                var errorMessage = $"无法完成 {apiUseCase}。chaty api: {apiPath}；响应代码：{response.StatusCode}；响应内容：{responseString}";
                _logger.LogWarning(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            return responseStream;
        }

        private async Task<string> InvokeChatyStringApi(string apiPath, HttpMethod httpMethod, HttpContent requestPayload, string useCaseDescription = null)
        {
            var apiInvocation = await Invoke(apiPath, httpMethod, requestPayload, useCaseDescription);
            var response = apiInvocation.Item1;
            if (response == null)
            {
                return null;
            }

            string responseString;
            using (var reader = new StreamReader(apiInvocation.Item2, Encoding.UTF8))
            {
                responseString = reader.ReadToEnd();
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"无法完成 {useCaseDescription}。chaty api: {apiPath}；响应代码：{response.StatusCode}；响应内容：{responseString}");
                return null;
            }
            return responseString;
        }

        private async Task<Tuple<HttpResponseMessage, Stream>> Invoke(string apiPath, HttpMethod httpMethod, HttpContent requestPayload, string useCaseDescription = null)
        {
            if (useCaseDescription == null)
            {
                useCaseDescription = "调用 Chaty API";
            }
            
            if (string.IsNullOrEmpty(_chatyOptions.ServiceBaseUrl))
            {
                _logger.LogWarning($"无法完成 {useCaseDescription}，因为尚未配置 chaty 服务所在的位置");
                return Tuple.Create<HttpResponseMessage, Stream>(null, null);
            }
            
            apiPath = apiPath.TrimStart('/');
            var serviceBaseUrl = _chatyOptions.ServiceBaseUrl.TrimEnd('/');
            var chatyApiUrl = $"{serviceBaseUrl}/{apiPath}";

            var request = new HttpRequestMessage();
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _chatyOptions.ApiToken);
            request.Method = httpMethod;
            request.RequestUri = new Uri(chatyApiUrl);
            if (requestPayload != null)
            {
                request.Content = requestPayload;
            }

            var response = await _httpClient.SendAsync(request, CancellationToken.None);
            var responseStream = await response.Content.ReadAsStreamAsync();
            return Tuple.Create(response, responseStream);
        }
    }
}