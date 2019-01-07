using System;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Admin.Controllers
{
    [Route("api")]
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

        [Route("ping")]
        public object Ping()
        {
            return new { Ping = "pong" };
        }
        
        [Route("api-object")]
        public ApiResponse ApiObj()
        {
            return ApiResponse.ActionResult(new { field = "foo" });
        }
        
        [Route("require-auth")]
        [Authorize]
        public ApiResponse RequireAuth()
        {
            return ApiResponse.NoContent();
        }
    }
}