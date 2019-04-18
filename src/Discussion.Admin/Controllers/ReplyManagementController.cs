using System.Linq;
using System.Net;
using Discussion.Admin.ViewModels;
using Discussion.Core.Data;
using Discussion.Core.Markdown;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Admin.Controllers
{
    [Authorize]
    public class ReplyManagementController: ControllerBase
    {
        private readonly IRepository<Topic> _topicRepo;
        private readonly IRepository<Reply> _replyRepo;

        public ReplyManagementController(IRepository<Topic> topicRepo, IRepository<Reply> replyRepo)
        {
            _topicRepo = topicRepo;
            _replyRepo = replyRepo;
        }

        [Route("api/topics/{topicId}/replies")]
        [HttpGet]
        public ApiResponse TopicReplies(int topicId)
        {
            var topic = _topicRepo.Get(topicId);
            if (topic == null)
            {
                return ApiResponse.NoContent(HttpStatusCode.NotFound);
            }

             
            var replies = _replyRepo.All()
                .Where(c => c.TopicId == topicId)
                .OrderBy(c => c.CreatedAtUtc)
                    .Include(r => r.CreatedByUser)
                    .Include(r => r.CreatedByWeChatAccount)
                .ToList()
                .Select(r =>
                    new ReplySummary
                    {
                        Id = r.Id,
                        TopicId = topicId,
                        MarkdownContent = r.Content,
                        HtmlContent = r.Content.MdToHtml(),
                        Author = new AuthorSummary
                        {
                            Id = r.CreatedByUser?.Id ?? 0, // todo: present wechat account
                            DisplayName = r.Author.DisplayName
                        },
                        CreatedAt = r.CreatedAtUtc
                    })
                .ToList();
            return ApiResponse.ActionResult(replies);
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
    }
}