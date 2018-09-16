using Discussion.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Discussion.Web.ViewModels;
using System;
using Discussion.Web.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Jusfr.Persistent;
using Microsoft.AspNetCore.Authorization;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {

        private readonly IRepository<Topic> _topicRepo;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        public TopicController(IRepository<Topic> topicRepo, IModelMetadataProvider modelMetadataProvider)
        {
            _topicRepo = topicRepo;
            _modelMetadataProvider = modelMetadataProvider;
        }
        

        [HttpGet]
        [Route("/")]
        [Route("/topics")]
        public ActionResult List()
        {
            var topicList = _topicRepo.All.ToList();

            return View(topicList);
        }

        [Route("/topics/{id}")]
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
                CreatedBy = HttpContext.DiscussionUser().User.Id,
                CreatedAt = DateTime.UtcNow
            };

            _topicRepo.Create(topic);
            return RedirectToAction("Index", new { topic.Id });
        }
    }

}