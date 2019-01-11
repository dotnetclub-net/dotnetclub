using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Discussion.Admin.ViewModels;
using Discussion.Core.Data;
using Discussion.Core.Markdown;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Admin.Controllers
{
    [Authorize]
    public class TopicManagementController: ControllerBase
    {
        const int TopicPageSize = 20;
        private readonly IRepository<Topic> _topicRepo;
        private readonly IRepository<Reply> _replyRepo;

        public TopicManagementController(IRepository<Topic> topicRepo, IRepository<Reply> replyRepo)
        {
            _topicRepo = topicRepo;
            _replyRepo = replyRepo;
        }

        [Route("api/topics")]
        public Paged<TopicItemSummary> List(int? page = 1)
        {
            return _topicRepo.All()
                            .OrderByDescending(topic => topic.Id)
                            .Page(SummarizeTopic(), 
                                  TopicPageSize, page);
        }
        
        [Route("api/topics/{id}")]
        public ApiResponse ShowDetail(int id)
        {
            var topic = _topicRepo.All()
                .Where(t => t.Id == id)
                    .Include(t => t.Author)
                .SingleOrDefault();

            if (topic == null)
            {
                return ApiResponse.NoContent(HttpStatusCode.NotFound);
            }


            return ApiResponse.ActionResult(new {
                Author = new UserSummary
                {
                    Id = topic.Author.Id,
                    DisplayName = topic.Author.DisplayName
                },
                CreatedAt = topic.CreatedAtUtc,
                Title = topic.Title,
                MarkdownContent = topic.Content,
                HtmlContent = topic.Content.MdToHtml()
            });
        }
        
        [Route("api/topics/{id}")]
        [HttpDelete]
        public ApiResponse Delete(int id)
        {
            var topic = _topicRepo.Get(id);
            if (topic == null)
            {
                return ApiResponse.NoContent(HttpStatusCode.NotFound);
            }
            
            _topicRepo.Delete(topic);
            var replies = _replyRepo.All().Where(r => r.Id == id).ToList();
            replies.ForEach(_replyRepo.Delete);
            return ApiResponse.NoContent();
        }


        static Expression<Func<Topic, TopicItemSummary>> SummarizeTopic()
        {
            return topic => new TopicItemSummary
            {
                Id = topic.Id,
                Title = topic.Title,
                ViewCount = topic.ViewCount,
                ReplyCount = topic.ReplyCount,
                Author = new UserSummary
                {
                    Id = topic.Author.Id,
                    DisplayName = topic.Author.DisplayName
                }
            };
        }
    }
}