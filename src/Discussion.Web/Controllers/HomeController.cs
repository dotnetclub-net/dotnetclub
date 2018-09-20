using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace Discussion.Web.Controllers
{
    public class HomeController: Controller
    {
       
        [Route("/about")]
        public ActionResult About()
        {
            return View();
        }

        [Route("/error")]
        public IActionResult Error()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }
    }
}
