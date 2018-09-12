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
        public ViewResult Signin()
        {
            return View();
        }
        
        
        [HttpPost]
        [Route("/signin")]        
        public async Task<IActionResult> DoSignin([FromForm]SigninUserViewModel viewModel, [FromQuery]string returnUrl)
        {
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

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }
            return Redirect(returnUrl);
        }
    }
}