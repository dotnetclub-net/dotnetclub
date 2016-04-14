using Discussion.Web.Models;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using System.Net;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Error()
        {
            await DiagnosticExceptionDetails();

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }

        async Task DiagnosticExceptionDetails()
        {
            return;

            var errorHandler = HttpContext.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;
            if(errorHandler == null)
            {
                return;
            }

            var error = errorHandler.Error;
            await Response.WriteAsync(error.Message);
            await Response.WriteAsync(error.StackTrace);
        }
    }
}
