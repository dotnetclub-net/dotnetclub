using Discussion.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Discussion.Web.Data;
using Discussion.Web.ViewModels;
using System;
using Discussion.Web.Services;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {

        private readonly IDataRepository<Topic> _topicRepo;
        public TopicController(IDataRepository<Topic> topicRepo)
        {
            _topicRepo = topicRepo;
        }


        [Route("/Topic/{id}")]
        public ActionResult Index(int id)
        {
            var topic = _topicRepo.Retrive(id);
            if(topic == null)
            {
                return NotFound();
            }

            var markdownRenderer = new MarkdownRenderService();
            var showModel = new TopicShowModel
            {
                Id = topic.Id,
                Title = topic.Title,
                MarkdownContent = topic.Content,
                HtmlContent = markdownRenderer.RenderMarkdownAsHtml(topic.Content)
            };

            return View(showModel);
        }


        [Route("/")]
        [Route("/Topic/List")]
        public ActionResult List()
        {
            var topicList = _topicRepo.All.ToList();

            return View(topicList);
        }


        [Route("/Topic/Create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("/Topic/CreateTopic")]
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
                TopicType = TopicType.Sharing,
                CreatedAt = DateTime.UtcNow
            };

            _topicRepo.Create(topic);
            return RedirectToAction("Index", new { topic.Id });
        }
    }

}