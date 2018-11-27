using Microsoft.AspNetCore.Mvc;
using Discussion.Web.ViewModels;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Discussion.Web.Models;
using Discussion.Web.Services.TopicManagement;
using Discussion.Web.Services.UserManagement.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {
        private const int PageSize = 20;
        private readonly IRepository<Topic> _topicRepo;
        private readonly ITopicService _topicService;

        public TopicController(IRepository<Topic> topicRepo, ITopicService topicService)
        {
            _topicRepo = topicRepo;
            _topicService = topicService;
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
            var showModel = _topicService.ViewTopic(id);
            if (showModel == null)
            {
                return NotFound();
            }

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

            try
            {
                var topic = _topicService.CreateTopic(model);
                return RedirectToAction("Index", new { topic.Id });
            }
            catch (UserVerificationRequiredException)
            {
                return BadRequest();
            }
        }
    }
}