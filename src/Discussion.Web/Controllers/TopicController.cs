using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {

        [Route("/")]
        public ActionResult List()
        {
            return View();    
        }
    }
}
