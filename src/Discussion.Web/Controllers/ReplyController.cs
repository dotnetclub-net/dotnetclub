using System;
using Discussion.Core.Data;
using Discussion.Core.Logging;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Web.Models;
using Discussion.Web.Services.UserManagement.Exceptions;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Controllers
{
    public class ReplyController : Controller
    {
        private readonly IRepository<Reply> _replyRepo;
        private readonly IRepository<Topic> _topicRepo;
        private readonly SiteSettings _siteSettings;
        private readonly ILogger<ReplyController> _logger;

        public ReplyController(IRepository<Reply> replyRepo, 
            IRepository<Topic> topicRepo,
            SiteSettings siteSettings, ILogger<ReplyController> logger)
        {
            _replyRepo = replyRepo;
            _topicRepo = topicRepo;
            _siteSettings = siteSettings;
            _logger = logger;
        }

        [Route("/topics/{topicId}/replies")]
        [HttpPost]
        [Authorize]
        public IActionResult Reply(int topicId, ReplyCreationModel replyCreationModel)
        {
            var currentUser = HttpContext.DiscussionUser();

            if (_siteSettings.RequireUserPhoneNumberVerified && !currentUser.PhoneNumberId.HasValue)
            {
                var ex = new UserVerificationRequiredException();
                _logger.LogWarning($"添加回复失败：{currentUser.UserName}：{ex.Message}");
                return BadRequest();
            }
            
            var topic = _topicRepo.Get(topicId);
            if (topic == null)
            {
                ModelState.AddModelError("TopicId", "话题不存在");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogModelState("添加回复", ModelState, currentUser.UserName);
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
            topic.LastRepliedAt = DateTime.UtcNow;
            topic.LastRepliedUser = currentUser;
            topic.ReplyCount += 1;
            _topicRepo.Update(topic);
            
            _logger.LogInformation($"添加回复成功：{currentUser.UserName}：(topicId: {topic.Id} replyId: {reply.Id})");
            return NoContent();
        }
    }
}