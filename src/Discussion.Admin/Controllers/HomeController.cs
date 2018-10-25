using System;
using Discussion.Admin.Models;
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
        
        
        [Route("error")]
        public ApiResponse Error()
        {
            throw new Exception("A server error has occured");
        }
    }
}