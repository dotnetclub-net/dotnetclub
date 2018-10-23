using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Web.Services.Emailconfirmation;
using Discussion.Web.Services.Identity;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IRepository<EmailBindOptions> _emailBindRepo;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;
        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender,
            IRepository<EmailBindOptions> emailBindRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _emailBindRepo = emailBindRepo;
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

        [Route("/setting")]
        public IActionResult Setting()

        {
            if (!HttpContext.IsAuthenticated())
            {
                return RedirectTo("/");
            }

            return View();
        }
        [HttpPost]
        [Route("/setting")]
        public async Task<IActionResult> DoSetting(EmailSettingViewModel emailSettingViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Setting");
            }
            var user = HttpContext.DiscussionUser();
            if (string.IsNullOrEmpty(user.EmailAddress))
            {
                //添加邮箱
                user.EmailAddress = emailSettingViewModel.EmailAddress;
                var result = _userManager.UpdateAsync(user);

                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _emailBindRepo.Save(new EmailBindOptions
                {
                    UserId = user.Id,
                    EmailAddress = emailSettingViewModel.EmailAddress,
                    OldEmailAddress = "",
                    CallbackToken = code,
                    IsActivation = false,
                    CreatedAtUtc = DateTime.Now
                });
                //发送邮件
                var callBack = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
                StringBuilder body = new StringBuilder();
                body.AppendLine("尊敬的用户请您妥善保存如下信息：");
                body.Append("你的客户端ID：").AppendLine("111");
                body.Append("你的客户端密钥：").AppendLine("222");
                body.AppendLine("请通过单击 <a href=\"" + callBack + "\">这里</a>来确认你的帐户");
                await _emailSender.SendEmailAsync(emailSettingViewModel.EmailAddress, "dotnetclub用户确认", body.ToString());
            }
            else
            {
                //更新邮箱
            }
            return RedirectTo("/");
        }
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = _userManager.Users.Where(t => t.Id.Equals(userId)).FirstOrDefault();
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "Register" : "Error");
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
