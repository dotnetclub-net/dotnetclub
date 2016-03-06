using Discussion.Web.Models;
using Microsoft.AspNet.Mvc;

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
            return new HttpStatusCodeResult(200);
        }
    }
}
