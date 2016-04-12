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

        // Use Topic/List to host home page.
        //[Route("/")]
        //public ActionResult Index()
        //{
        //    return View();
        //}



        [Route("/About")]
        public  ActionResult About()
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
