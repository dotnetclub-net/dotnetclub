using Microsoft.AspNetCore.Mvc;
using Discussion.Web.ViewModels;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {
        private const int PageSize = 20;
        private readonly IRepository<Topic> _topicRepo;
        private readonly IRepository<Reply> _replyRepo;
        private readonly IConfigurationRoot _appConfig;

        public TopicController(IRepository<Topic> topicRepo, IRepository<Reply> replyRepo, IConfigurationRoot appConfig)
        {
            _topicRepo = topicRepo;
            _replyRepo = replyRepo;
            _appConfig = appConfig;
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
            var currentUser = HttpContext.DiscussionUser();
            
            var requireVerifiedNumber = Convert.ToBoolean(_appConfig[UserController.ConfigKeyRequireUserPhoneNumberVerified]);
            if (requireVerifiedNumber && !currentUser.PhoneNumberId.HasValue)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // ReSharper disable once PossibleInvalidOperationException
            var topic = new Topic
            {
                Title = model.Title,
                Content = model.Content,
                Type = model.Type.Value,
                CreatedBy = currentUser.Id,
                CreatedAtUtc = DateTime.UtcNow
            };

            _topicRepo.Save(topic);
            return RedirectToAction("Index", new { topic.Id });
        }
    }
}