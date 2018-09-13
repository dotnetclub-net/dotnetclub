using System;
using System.Threading.Tasks;
using Discussion.Web.Models;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Controllers
{
    public class AccountController: Controller
    {
        [HttpGet]
        [Route("/signin")]
        public IActionResult Signin([FromQuery]string returnUrl)
        {
            if (IsAuthenticated())
            {
                return RedirectTo(returnUrl);
            }
            return View();
        }


        [HttpPost]
        [Route("/signin")]        
        public async Task<IActionResult> DoSignin([FromForm]SigninUserViewModel viewModel, [FromQuery]string returnUrl)
        {
            if (IsAuthenticated())
            {
                return RedirectTo(returnUrl);
            }
            
            if (!ModelState.IsValid)
            {
                return View("Signin");
            }
            
            var user = new User
            {
                DisplayName = viewModel.UserName,
                UserName = viewModel.UserName
            };
            await this.HttpContext.SigninAsync(user);

            return RedirectTo(returnUrl);
        }

        
        [HttpPost]
        [Route("/signout")] 
        public async Task<IActionResult> DoSignOut()
        {
            var redirectToHome = RedirectTo("/");
            if (!IsAuthenticated())
            {
                return redirectToHome;
            }

            await this.HttpContext.SignoutAsync();
            return redirectToHome;
        }
        
        
        private IActionResult RedirectTo(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }

            return Redirect(returnUrl);
        }

        private bool IsAuthenticated()
        {
            var isAuthedExpr = HttpContext?.User?.Identity?.IsAuthenticated;
            var isAuthed = isAuthedExpr.HasValue && isAuthedExpr.Value;
            return isAuthed;
        }
    }
}
