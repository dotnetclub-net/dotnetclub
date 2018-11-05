using System;
using System.Linq;
using System.Linq.Expressions;
using Discussion.Admin.ViewModels;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Admin.Controllers
{
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