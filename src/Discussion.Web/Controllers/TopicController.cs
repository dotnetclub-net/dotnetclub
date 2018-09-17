using Discussion.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Discussion.Web.ViewModels;
using System;
using Discussion.Web.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Jusfr.Persistent;
using Markdig;
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


        private const int PageSize = 20;

        [HttpGet]
        [Route("/")]
        [Route("/topics")]
        public ActionResult List([FromQuery]int? page = null)
        {
            var topicCount = _topicRepo.All.Count();
            var actualPage = NormalizePaging(page, topicCount, out var allPage);

            var topicList = _topicRepo.All
                                      .OrderByDescending(topic => topic.CreatedAt)
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
                HtmlContent = Markdown.ToHtml(topic.Content ?? string.Empty)
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
        
        
        
        static int NormalizePaging(int? page, int topicCount, out int allPage)
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