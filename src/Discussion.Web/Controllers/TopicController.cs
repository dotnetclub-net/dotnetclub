using Microsoft.AspNetCore.Mvc;
using Discussion.Web.ViewModels;
using System;
using Discussion.Web.Services.Identity;
using Discussion.Web.Services.Markdown;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {
        private readonly IRepository<Topic> _topicRepo;
        private readonly IRepository<Reply> _replyRepo;

        public TopicController(IRepository<Topic> topicRepo, IRepository<Reply> replyRepo)
        {
            _topicRepo = topicRepo;
            _replyRepo = replyRepo;
        }

        private const int PageSize = 20;

        [HttpGet]
        [Route("/")]
        [Route("/topics")]
        public ActionResult List([FromQuery]int? page = null)
        {
            var topicCount = _topicRepo.All().Count();
            var actualPage = NormalizePaging(page, topicCount, out var allPage);

            var topicList = _topicRepo.All()
                                      .OrderByDescending(topic => topic.CreatedAtUtc)
                                      .Skip((actualPage - 1) * PageSize)
                                      .Take(PageSize)
                                      .ToList();

            var listModel = new TopicListViewModel
            {
                Topics = topicList,
                CurrentPage = actualPage,
                HasPreviousPage = actualPage > 1,
                HasNextPage = actualPage < allPage
            };
            return View(listModel);
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

        private static int NormalizePaging(int? page, int topicCount, out int allPage)
        {
            var actualPage = 0;

            if (page == null || page.Value < 1)
            {
                actualPage = 1;
            }
            else
            {
                actualPage = page.Value;
            }

            var basePage = topicCount / PageSize;
            allPage = topicCount % PageSize == 0 ? basePage : basePage + 1;
            if (actualPage > allPage)
            {
                actualPage = allPage;
            }

            return actualPage;
        }
    }
}