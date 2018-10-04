using System;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Web.Services.Identity;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Controllers
{
    public class AccountController: Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User>_signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager, 
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        
        [Route("/signin")]
        public IActionResult Signin([FromQuery]string returnUrl)
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectTo(returnUrl);
            }
            return View();
        }


        [HttpPost]
        [Route("/signin")]        
        public async Task<IActionResult> DoSignin([FromForm]SigninUserViewModel viewModel, [FromQuery]string returnUrl)
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectTo(returnUrl);
            }

            var result = Microsoft.AspNetCore.Identity.SignInResult.Failed;
            if (ModelState.IsValid)
            {
                result = await _signInManager.PasswordSignInAsync(
                    viewModel.UserName,
                    viewModel.Password,
                    isPersistent: false,
                    lockoutOnFailure: true);
                
                _logger.LogInformation($"用户 {viewModel.UserName} 尝试登录，结果 {result}");
            }
            else
            {
                _logger.LogInformation($"用户 {viewModel.UserName} 尝试登录，但用户名密码的格式不正确");
            }

            if (!result.Succeeded)
            {
                ModelState.Clear();   // 将真正的验证结果隐藏掉（如果有的话）
                ModelState.AddModelError("UserName", "用户名或密码错误");
            }

            return ModelState.IsValid ? RedirectTo(returnUrl) : View("Signin");
        }

        
        [HttpPost]
        [Route("/signout")] 
        public async Task<IActionResult> DoSignOut()
        {
            var redirectToHome = RedirectTo("/");
            if (!HttpContext.IsAuthenticated())
            {
                return redirectToHome;
            }

            await _signInManager.SignOutAsync();
            return redirectToHome;
        }
        
        
        [Route("/register")]  
        public IActionResult Register()
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectTo("/");
            }
            
            return View();
        }
        
        
        [HttpPost]
        [Route("/register")]  
        public async Task<IActionResult> DoRegister(SigninUserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Register");
            }
            
            var newUser = new User
            {
                UserName = userViewModel.UserName,
                DisplayName = userViewModel.UserName,
                CreatedAtUtc = DateTime.UtcNow
            };
            
            var result = await _userManager.CreateAsync(newUser, userViewModel.Password);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(";", result.Errors.Select(err => err.Description));
                ModelState.AddModelError("UserName", errorMessage);
                return View("Register");
            }
            
            await _signInManager.PasswordSignInAsync(
                userViewModel.UserName,
                userViewModel.Password,
                isPersistent: false,
                lockoutOnFailure: true);
            return RedirectTo("/");
        }
        
        
        private IActionResult RedirectTo(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }

            return Redirect(returnUrl);
        }
    }
}
