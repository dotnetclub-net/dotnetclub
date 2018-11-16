using Microsoft.AspNetCore.Mvc;
using Discussion.Web.ViewModels;
using System;
using System.Collections.Generic;
using Discussion.Web.Services.Identity;
using Discussion.Web.Services.Markdown;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {
        private const int PageSize = 20;
        private readonly IRepository<Topic> _topicRepo;
        private readonly IRepository<Reply> _replyRepo;

        public TopicController(IRepository<Topic> topicRepo, IRepository<Reply> replyRepo)
        {
            _topicRepo = topicRepo;
            _replyRepo = replyRepo;
        }


        [HttpGet]
        [Route("/")]
        [Route("/topics")]
        public ActionResult List([FromQuery]int? page = null)
        {
            var pagedTopics = _topicRepo.All()
                                        .Include(t => t.Author)
                                        .Include(t => t.LastRepliedUser)
                                        .OrderByDescending(topic => topic.CreatedAtUtc)
                                        .Page(PageSize, page);

            return View(pagedTopics);
        }

        [Route("/topics/{id}")]
        public ActionResult Index(int id)
        {
            var topic = _topicRepo.All()
                .Where(t => t.Id == id)
                .Include(t => t.Author)
                .SingleOrDefault();

            if (topic == null)
            {
                return NotFound();
            }

            var replies = _replyRepo.All()
                                        .Where(c => c.TopicId == id)
                                        .OrderBy(c => c.CreatedAtUtc)
                                        .Include(r => r.Author)
                                        .ToList();
            var showModel = TopicViewModel.CreateFrom(topic, replies);

            topic.ViewCount++;
            _topicRepo.Update(topic);
            return View(showModel);
        }

        [Authorize]
        [Route("/topics/create")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [Route("/topics")]
        public ActionResult CreateTopic(TopicCreationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var topic = new Topic
            {
                Title = model.Title,
                Content = model.Content,
                Type = model.Type.Value,
                CreatedBy = HttpContext.DiscussionUser().Id,
                CreatedAtUtc = DateTime.UtcNow
            };

            _topicRepo.Save(topic);
            return RedirectToAction("Index", new { topic.Id });
        }
    }
}