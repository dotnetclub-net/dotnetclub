using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Controllers
{
    public class HomeController: Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        [Route("/about")]
        public ActionResult About()
        {
            return View();
        }

        [Route("/error")]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if(exceptionFeature != null && exceptionFeature.Error != null)
            { 
                _logger.LogError(exceptionFeature.Error, "服务器错误");
            }
            
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }
    }
}
