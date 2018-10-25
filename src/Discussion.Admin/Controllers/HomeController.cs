using Microsoft.AspNetCore.Mvc;

namespace Discussion.Admin.Controllers
{
    public class HomeController : ControllerBase
    {
        [Route("admin-home")]
        public ContentResult Index()
        {
            return new ContentResult
            {
                Content = "Hello Admin"
            }; 
        }
    }
}