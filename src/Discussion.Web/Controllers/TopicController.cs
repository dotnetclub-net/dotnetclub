using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Logging;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Discussion.Web.Services.ChatHistoryImporting;
using Discussion.Web.Services.TopicManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {
        private const int PageSize = 20;
        private readonly IRepository<Topic> _topicRepo;
        private readonly ITopicService _topicService;
        private readonly ILogger<TopicController> _logger;
        private readonly ChatyOptions _chatyOptions;
        private readonly IChatHistoryImporter _chatHistoryImporter;
        private readonly HttpMessageInvoker _httpClient;
        private readonly IRepository<Reply> _replyRepo;
        private readonly IRepository<WeChatAccount> _wechatAccountRepo;

        public TopicController(IRepository<Topic> topicRepo, 
            ITopicService topicService, 
            ILogger<TopicController> logger,
            IOptions<ChatyOptions> chatyOptions, IChatHistoryImporter chatHistoryImporter, HttpMessageInvoker httpClient, IRepository<Reply> replyRepo, IRepository<WeChatAccount> wechatAccountRepo)
        {
            _topicRepo = topicRepo;
            _topicService = topicService;
            _logger = logger;
            _chatyOptions = chatyOptions?.Value;
            _chatHistoryImporter = chatHistoryImporter;
            _httpClient = httpClient;
            _replyRepo = replyRepo;
            _wechatAccountRepo = wechatAccountRepo;
        }


        [HttpGet]
        [Route("/")]
        [Route("/topics")]
        public ActionResult List([FromQuery]int? page = null)
        {
            var pagedTopics = _topicRepo.All()
                                        .Include(t => t.CreatedByUser)
                                            .ThenInclude(u => u.AvatarFile)
                                        .Include(t => t.LastRepliedByUser)
                                            .ThenInclude(u => u.AvatarFile)
                                        .Include(t => t.LastRepliedByWeChatAccount)
                                            .ThenInclude(u => u.AvatarFile)
                                        .OrderByDescending(topic => topic.CreatedAtUtc)
                                        .Page(PageSize, page);

            return View(pagedTopics);
        }

        [Route("/topics/{id}")]
        public ActionResult Index(int id)
        {
            var showModel = _topicService.ViewTopic(id);
            if (showModel == null)
            {
                return NotFound();
            }

            return View(showModel);
        }

        [Authorize]
        [Route("/topics/create")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [Route("/topics")]
        public ActionResult CreateTopic(TopicCreationModel model)
        {
            var userName = HttpContext.DiscussionUser().UserName;
            if (!ModelState.IsValid)
            {
                _logger.LogModelState("创建话题", ModelState, userName);
                return BadRequest();
            }

            try
            {
                var topic = _topicService.CreateTopic(model);
                _logger.LogInformation($"创建话题成功：{userName}：{topic.Title}(id: {topic.Id})");
                return RedirectToAction("Index", new { topic.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"创建话题失败：{userName}：{ex.Message}");
                return BadRequest();
            }
        }
        
        [Authorize]
        [HttpPost]
        [Route("/topics/import-from-wechat")]
        public async Task<ActionResult> ImportFromWeChat(ChatHistoryImportingModel model)
        {
            if (string.IsNullOrEmpty(_chatyOptions.ServiceBaseUrl))
            {
                _logger.LogWarning("无法导入对话，因为尚未配置 chaty 服务所在的位置");
                return BadRequest();
            }

            var userId = HttpContext.DiscussionUser().Id;
            var weChatAccount = _wechatAccountRepo.All().FirstOrDefault(wxa => wxa.UserId == userId);
            if (weChatAccount == null)
            {
                _logger.LogWarning("无法导入对话，因为当前用户未绑定微信号");
                return BadRequest();
            }
            
            var serviceBaseUrl = _chatyOptions.ServiceBaseUrl.TrimEnd('/');
            var apiPath = $"{serviceBaseUrl}/chat/detail/{weChatAccount.WxId}/{model.ChatId}";
            
            var chatDetailRequest = new HttpRequestMessage();
            chatDetailRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", _chatyOptions.ApiToken);
            chatDetailRequest.Method = HttpMethod.Get;
            chatDetailRequest.RequestUri = new Uri(apiPath);

            var response = await _httpClient.SendAsync(chatDetailRequest, CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("无法导入对话 因为无法从 chaty 获取聊天内容");
                return BadRequest();
            }
            
            ChatMessage[] messages;
            var jsonStream = await response.Content.ReadAsStreamAsync();
            using (var reader = new StreamReader(jsonStream, Encoding.UTF8))
            {
                var jsonString = reader.ReadToEnd();
                messages = JsonConvert.DeserializeObject<ChatMessage[]>(jsonString, new ChatMessageContentJsonConverter());
            }

            var replies = await _chatHistoryImporter.Import(messages);
            var actionResult = CreateTopic(model);
            if (!(actionResult is RedirectToActionResult redirectResult))
            {
                return actionResult;
            }

            var topicId = (int)redirectResult.RouteValues["Id"];
            var topic = _topicRepo.Get(topicId);
            replies.ForEach(r =>
            {
                r.TopicId = topicId;
                _replyRepo.Save(r);
            });
            
            topic.ReplyCount = replies.Count;
            topic.LastRepliedByWeChatAccount = replies.Last().CreatedByWeChatAccount;
            topic.LastRepliedAt = replies.Last().CreatedAtUtc;
            _topicRepo.Update(topic);
            return redirectResult;
        }
    }
}