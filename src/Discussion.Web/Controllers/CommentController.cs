using System;
using Discussion.Web.Data;
using Discussion.Web.Models;
using Discussion.Web.Services.Identity;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Controllers
{
    public class CommentController: Controller
    {
        private IRepository<Comment> _commentRepo;
        private IRepository<Topic> _topicRepo;

        public CommentController(IRepository<Comment> commentRepo, IRepository<Topic> topicRepo)
        {
            _commentRepo = commentRepo;
            _topicRepo = topicRepo;
        }


        [Route("/topics/{topicId}/comments")]
        [HttpPost]
        [Authorize]
        public IActionResult Comment(int topicId, CommentCreationModel commentCreationModel)
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
            
            var comment = new Comment
            {
                TopicId = topicId,
                CreatedBy = User.ExtractUserId().Value,
                Content = commentCreationModel.Content
            };
            _commentRepo.Save(comment);
            return NoContent();
        }
    }
}