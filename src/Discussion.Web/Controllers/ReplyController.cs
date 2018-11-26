using System;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Web.Models;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Controllers
{
    public class ReplyController : Controller
    {
        private readonly IRepository<Reply> _replyRepo;
        private readonly IRepository<Topic> _topicRepo;
        private SiteSettings _siteSettings;

        public ReplyController(IRepository<Reply> replyRepo, 
            IRepository<Topic> topicRepo,
            SiteSettings siteSettings)
        {
            _replyRepo = replyRepo;
            _topicRepo = topicRepo;
            _siteSettings = siteSettings;
        }

        [Route("/topics/{topicId}/replies")]
        [HttpPost]
        [Authorize]
        public IActionResult Reply(int topicId, ReplyCreationModel replyCreationModel)
        {
            var currentUser = HttpContext.DiscussionUser();

            if (_siteSettings.RequireUserPhoneNumberVerified && !currentUser.PhoneNumberId.HasValue)
            {
                return BadRequest();
            }
            
            var topic = _topicRepo.Get(topicId);
            if (topic == null)
            {
                ModelState.AddModelError("TopicId", "话题不存在");
            }

            if (!ModelState.IsValid)
            {
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
            return NoContent();
        }
    }
}