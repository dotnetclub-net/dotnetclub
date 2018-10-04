using System;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Web.Services.Identity;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Controllers
{
    public class ReplyController : Controller
    {
        private IRepository<Reply> _replyRepo;
        private IRepository<Topic> _topicRepo;

        public ReplyController(IRepository<Reply> replyRepo, IRepository<Topic> topicRepo)
        {
            _replyRepo = replyRepo;
            _topicRepo = topicRepo;
        }

        [Route("/topics/{topicId}/replies")]
        [HttpPost]
        [Authorize]
        public IActionResult Reply(int topicId, ReplyCreationModel replyCreationModel)
        {
            var topic = _topicRepo.Get(topicId);
            if (topic == null)
            {
                ModelState.AddModelError("TopicId", "话题不存在");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            topic.LastRepliedAt = DateTime.UtcNow;
            topic.ReplyCount += 1;
            _topicRepo.Update(topic);

            var reply = new Reply
            {
                TopicId = topicId,
                CreatedBy = User.ExtractUserId().Value,
                Content = replyCreationModel.Content
            };
            _replyRepo.Save(reply);
            return NoContent();
        }
    }
}