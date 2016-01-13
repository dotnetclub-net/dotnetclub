using Microsoft.AspNet.Mvc;

namespace Discussion.Web.Controllers
{
    public class HomeController: Controller
    {

        [Route("/")]
        public ActionResult Index()
        {
            return new HttpStatusCodeResult(200);
        }
    }
}
