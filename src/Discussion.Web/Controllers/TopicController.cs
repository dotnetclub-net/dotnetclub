using System;
using Microsoft.AspNetCore.Mvc;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Net;
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
        public ActionResult List([FromQuery] int? page = null)
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
            var user = HttpContext.DiscussionUser();
            var chatySupported = _chatyApiService.IsChatySupported(user.UserName);
            var weChatAccount = chatySupported
                ? _wechatAccountRepo.All().FirstOrDefault(wxa => wxa.UserId == user.Id)
                : null;

            return View(weChatAccount != null);
        }

        [Authorize]
        [HttpPost]
        [Route("/topics")]
        public ActionResult CreateTopic(TopicCreationModel model)
        {
            var user = HttpContext.DiscussionUser();
            var userName = user.UserName;
            if (!ModelState.IsValid)
            {
                _logger.LogModelState("创建话题", ModelState, user.Id, userName);
                return BadRequest();
            }

            try
            {
                var topic = _topicService.CreateTopic(model);
                _logger.LogInformation("创建话题成功：{@NewTopicAttempt}",
                    new {topic.Title, topic.Id, UserId = user.Id, user.UserName});
                // ReSharper disable once Mvc.ActionNotResolved
                return RedirectToAction("Index", new {topic.Id});
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("创建话题失败：{@NewTopicAttempt}",
                    new {UserId = user.Id, user.UserName, Result = ex.Message});
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/topics/import-from-wechat")]
        public async Task<ActionResult> ImportFromWeChat(ChatHistoryImportingModel model)
        {
            var user = HttpContext.DiscussionUser();
            var weChatAccount = _wechatAccountRepo.All().FirstOrDefault(wxa => wxa.UserId == user.Id);
            if (weChatAccount == null)
            {
                _logger.LogWarning("导入对话失败：{@ImportAttempt}", new {UserId = user.Id, user.UserName, Result = "未绑定微信号"});
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

            var topicId = (int) redirectResult.RouteValues["Id"];
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

            _logger.LogInformation("导入对话成功：{@ImportAttempt}",
                new {TopicId = topic.Id, model.ChatId, topic.ReplyCount, UserId = user.Id, user.UserName});
            return redirectResult;
        }

        [Authorize]
        [Route("/topics/wechat-session-list")]
        public async Task<ApiResponse> GetWeChatSessionList()
        {
            var user = HttpContext.DiscussionUser();
            var weChatAccount = _wechatAccountRepo.All().FirstOrDefault(wxa => wxa.UserId == user.Id);
            if (weChatAccount == null)
            {
                _logger.LogWarning("加载对话列表失败：{@ImportAttempt}",
                    new {UserId = user.Id, user.UserName, Result = "未绑定微信号"});
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }

            var messageList = await _chatyApiService.GetMessageList(weChatAccount.WxId);
            return messageList == null
                ? ApiResponse.NoContent(HttpStatusCode.InternalServerError)
                : ApiResponse.ActionResult(messageList);
        }

        [Authorize]
        [HttpGet]
        [Route("/topics/profiles")]
        public IActionResult Profile([FromQuery] int? page = null)
        {
            var user = HttpContext.DiscussionUser();
            var tpoics = _topicRepo.All()
                .Include(t => t.CreatedByUser)
                .Where(t => t.CreatedByUser.Id == user.Id)
                .Select(entity => new TopicProfileViewModel
                {
                    Id = entity.Id,
                    Title = entity.Title,
                    Type = entity.Type,
                    CreateTime = entity.CreatedAtUtc,
                    ViewCount = entity.ViewCount,
                    ReplyCount = entity.ReplyCount
                }).Page(PageSize, page);
            return View(tpoics);
        }

        [Route("api/topics/{id}")]
        [HttpDelete]
        public ApiResponse Delete(int id)
        {
            //todo  user validate
            //var user = HttpContext.DiscussionUser();
            var topic = _topicRepo.Get(id);
            if (topic == null)
            {
                return ApiResponse.NoContent(HttpStatusCode.NotFound);
            }

            var replies = _replyRepo.All().Where(r => r.Id == id).ToList();
            if (replies.Count < 5 && (replies.Select(t => t.CreatedByUser.Id).Distinct().Count() < 5))
            {
                _topicRepo.Delete(topic);
                replies.ForEach(_replyRepo.Delete);
                return ApiResponse.NoContent();
            }

            return ApiResponse.Error("不符合删除条件");
        }

        [Route("api/topics/{id}")]
        [HttpPatch]
        public ApiResponse Update(int id)
        {
            return ApiResponse.NoContent();
        }
    }
}