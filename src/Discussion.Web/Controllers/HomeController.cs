using Discussion.Web.Models;
using Microsoft.AspNet.Mvc;
using System.Net;

namespace Discussion.Web.Controllers
{
    public class HomeController: Controller
    {
        // IRepository<Article, Article> 

        public HomeController()
        {

        }

        [Route("/")]
        public ActionResult Index()
        {
            return View();
        }



        [Route("/Error")]
        public IActionResult Error()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }
    }
}
