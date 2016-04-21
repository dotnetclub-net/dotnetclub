using Discussion.Web.Models;
using Microsoft.AspNet.Mvc;
using System.Linq;
using Discussion.Web.Data;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {

        private readonly IDataRepository<Topic> _topicRepo;
        public TopicController(IDataRepository<Topic> topicRepo)
        {
            _topicRepo = topicRepo;
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
    }
}