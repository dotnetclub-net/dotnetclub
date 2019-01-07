using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Discussion.Admin.ViewModels;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Admin.Controllers
{
    [Authorize]
    public class TopicManagementController: ControllerBase
    {
        const int TopicPageSize = 20;
        private readonly IRepository<Topic> _topicRepo;

        public TopicManagementController(IRepository<Topic> topicRepo)
        {
            _topicRepo = topicRepo;
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
        [HttpDelete]
        public ApiResponse Delete(int id)
        {
            var topic = _topicRepo.Get(id);
            if (topic == null)
            {
                return ApiResponse.NoContent(HttpStatusCode.NotFound);
            }
            
            _topicRepo.Delete(topic);
            return ApiResponse.NoContent();
        }


        static Expression<Func<Topic, TopicItemSummary>> SummarizeTopic()
        {
            return topic => new TopicItemSummary
            {
                Id = topic.Id,
                Title = topic.Title,
                Author = new UserSummery
                {
                    Id = topic.Author.Id,
                    DisplayName = topic.Author.DisplayName
                }
            };
        }
    }
}