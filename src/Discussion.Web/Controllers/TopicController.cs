using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {
        private const int PageSize = 20;
        private readonly IRepository<Topic> _topicRepo;
        private readonly ITopicService _topicService;
        private readonly ILogger<TopicController> _logger;
        private readonly IChatHistoryImporter _chatHistoryImporter;
        private readonly IRepository<Reply> _replyRepo;
        private readonly IRepository<WeChatAccount> _wechatAccountRepo;
        private readonly ChatyApiService _chatyApiService;

        public TopicController(IRepository<Topic> topicRepo, 
            ITopicService topicService, 
            ILogger<TopicController> logger, IChatHistoryImporter chatHistoryImporter,
            IRepository<Reply> replyRepo, IRepository<WeChatAccount> wechatAccountRepo,
            ChatyApiService chatyApiService)
        {
            _topicRepo = topicRepo;
            _topicService = topicService;
            _logger = logger;
            _chatHistoryImporter = chatHistoryImporter;
            _replyRepo = replyRepo;
            _wechatAccountRepo = wechatAccountRepo;
            _chatyApiService = chatyApiService;
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
            var userId = HttpContext.DiscussionUser().Id;
            var weChatAccount = _wechatAccountRepo.All().FirstOrDefault(wxa => wxa.UserId == userId);
            return View(weChatAccount != null);
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
            var userId = HttpContext.DiscussionUser().Id;
            var weChatAccount = _wechatAccountRepo.All().FirstOrDefault(wxa => wxa.UserId == userId);
            if (weChatAccount == null)
            {
                _logger.LogWarning("无法导入对话，因为当前用户未绑定微信号");
                return BadRequest();
            }

            var messages = await _chatyApiService.GetMessagesInChat(weChatAccount.WxId, model.ChatId);
            if (messages == null)
            {
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
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

        [Authorize]
        [Route("/topics/wechat-session-list")]
        public async Task<ApiResponse> GetWeChatSessionList()
        {
            var userId = HttpContext.DiscussionUser().Id;
            var weChatAccount = _wechatAccountRepo.All().FirstOrDefault(wxa => wxa.UserId == userId);
            if (weChatAccount == null)
            {
                _logger.LogWarning("无法导入对话，因为当前用户未绑定微信号");
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }

            var messageList = await _chatyApiService.GetMessageList(weChatAccount.WxId);
            return messageList == null 
                ? ApiResponse.NoContent(HttpStatusCode.InternalServerError) 
                : ApiResponse.ActionResult(messageList);
        }
    }
}