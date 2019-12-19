using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Logging;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Discussion.Core.Time;
using Discussion.Web.Services.UserManagement.Exceptions;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Controllers
{
    public class ReplyController : Controller
    {
        private const int PageSize = 20;
        private readonly IRepository<Reply> _replyRepo;
        private readonly IRepository<Topic> _topicRepo;
        private readonly SiteSettings _siteSettings;
        private readonly ILogger<ReplyController> _logger;
        private readonly IClock _clock;

        public ReplyController(IRepository<Reply> replyRepo, 
            IRepository<Topic> topicRepo,
            SiteSettings siteSettings, ILogger<ReplyController> logger, IClock clock)
        {
            _replyRepo = replyRepo;
            _topicRepo = topicRepo;
            _siteSettings = siteSettings;
            _logger = logger;
            _clock = clock;
        }

        [Route("/topics/{topicId}/replies")]
        [HttpPost]
        [Authorize]
        public IActionResult Reply(int topicId, ReplyCreationModel replyCreationModel)
        {
            var currentUser = HttpContext.DiscussionUser();

            if (!_siteSettings.CanAddNewReplies())
            {
                _logger.LogWarning("添加回复失败：{@ReplyAttempt}", new {currentUser.UserName, Result= new FeatureDisabledException().Message});
                return BadRequest();
            }

            if (_siteSettings.RequireUserPhoneNumberVerified && !currentUser.PhoneNumberId.HasValue)
            {
                _logger.LogWarning("添加回复失败：{@ReplyAttempt}", new {currentUser.UserName, Result = new UserVerificationRequiredException().Message});
                return BadRequest();
            }
            
            var topic = _topicRepo.Get(topicId);
            if (topic == null)
            {
                var errorMessage = "话题不存在";
                _logger.LogWarning("添加回复失败：{@ReplyAttempt}", new {currentUser.UserName, Result = errorMessage});
                ModelState.AddModelError("TopicId", errorMessage);
            }

            if (!ModelState.IsValid)
            {
                _logger.LogModelState("添加回复", ModelState, currentUser.Id, currentUser.UserName);
                return BadRequest(ModelState);
            }

            var reply = new Reply
            {
                TopicId = topicId,
                CreatedBy = currentUser.Id,
                Content = replyCreationModel.Content
            };
            _replyRepo.Save(reply);
            
            // ReSharper disable once PossibleNullReferenceException
            topic.LastRepliedAt = _clock.Now.UtcDateTime;
            topic.LastRepliedByUser = currentUser;
            topic.ReplyCount += 1;
            _topicRepo.Update(topic);
            
            _logger.LogInformation("添加回复成功：{@ReplyAttempt}", new {TopicId = topic.Id, topic.ReplyCount, ReplyId = reply.Id, UserId = currentUser.Id, currentUser.UserName});
            return NoContent();
        }

        [Authorize]
        [HttpGet]
        [Route("/topics/replies")]
        public IActionResult Replies([FromQuery] int? page = null)
        {
            var user = HttpContext.DiscussionUser();
            var replies = _replyRepo.All()
                .Include(t => t.CreatedByUser)
                .Where(t => t.CreatedByUser.Id == user.Id)
                .Select(entity => new ReplyProfileViewModel
                {
                    CreatedByUser = entity.CreatedByUser,
                    TopicId = entity.TopicId,
                    ReplyContent = entity.Content,
                    ReplyCreateTime = entity.CreatedAtUtc
                }).Page(PageSize, page);
            return View(replies);
        }
        
        [Route("api/topics/{topicId}/replies/{replyId}")]
        [HttpDelete]
        public ApiResponse Delete(int topicId, int replyId)
        {
            var topic = _topicRepo.Get(topicId);
            if (topic == null)
            {
                return ApiResponse.NoContent(HttpStatusCode.NotFound);
            }
            
            var reply = _replyRepo.All().FirstOrDefault(r => r.Id == replyId && r.TopicId == topicId);
            if (reply == null)
            {
                return ApiResponse.NoContent(HttpStatusCode.NotFound); 
            }
            
            _replyRepo.Delete(reply);


            topic.ReplyCount = _replyRepo.All().Count(r => r.TopicId == topicId);
            var latestReply = _replyRepo.All()
                .Where(r => r.TopicId == topicId)
                .OrderByDescending(r => r.CreatedAtUtc)
                .FirstOrDefault();
                
            topic.LastRepliedBy = latestReply?.CreatedBy;
            topic.LastRepliedAt = latestReply?.CreatedAtUtc;            
            _topicRepo.Update(topic);
            return ApiResponse.NoContent();
        }

        [Route("api/topics/{id}")]
        [HttpPatch]
        public ApiResponse Update(int topicId, int replyId)
        {
            return ApiResponse.NoContent();
        }
    }
}