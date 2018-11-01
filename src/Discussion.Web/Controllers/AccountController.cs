using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Web.Services.Emailconfirmation;
using Discussion.Web.Services.Identity;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Discussion.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IRepository<EmailBindOptions> _emailBindRepo;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;
        private IDataProtector _protector;
        private AuthMessageSenderOptions _authMessageSenderOptions;
        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender,
            IRepository<EmailBindOptions> emailBindRepo,
            IDataProtectionProvider provider,
            IOptions<AuthMessageSenderOptions> authMessageSenderOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _emailBindRepo = emailBindRepo;
            _protector = provider.CreateProtector(authMessageSenderOptions.Value.EncryptionKey);
            _authMessageSenderOptions = authMessageSenderOptions.Value;
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
            var user = HttpContext.DiscussionUser();
            return View(user);
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
            //添加邮箱 不能绑定旧邮箱
            string oldEmailAddress = string.Empty;
            if (!string.IsNullOrWhiteSpace(user.EmailAddress))
            {
                var eamilBindOption = _emailBindRepo.All().FirstOrDefault(t => t.EmailAddress.Equals(user.EmailAddress.ToLower()));
                oldEmailAddress = eamilBindOption.OldEmailAddress;
                if (oldEmailAddress != null && oldEmailAddress.Equals(emailSettingViewModel.EmailAddress))
                    return View("Setting");
                else
                    oldEmailAddress = user.EmailAddress;
            }
            user.EmailAddress = emailSettingViewModel.EmailAddress;
            var result = _userManager.UpdateAsync(user);
            string initialToken = $"{user.Id.ToString()}|dotnetclub";
            string sendToken = _protector.Protect(initialToken);
            _emailBindRepo.Save(new EmailBindOptions
            {
                UserId = user.Id,
                EmailAddress = emailSettingViewModel.EmailAddress,
                OldEmailAddress = oldEmailAddress,
                CallbackToken = initialToken,
                IsActivation = false,
                CreatedAtUtc = DateTime.Now
            });
            //发送邮件
            var callBack = Url.Action(
                action: "ConfirmEmail",
                controller: "Account",
                values: new { code = sendToken },
                protocol: Request.Scheme);
            var sendBody = EmailExtensions.SplicedMailTemplate(HtmlEncoder.Default.Encode(callBack));
            await _emailSender.SendEmailAsync(emailSettingViewModel.EmailAddress, "dotnetclub用户确认", sendBody);
            return RedirectTo("/setting");
        }
        [Route("/[controller]/[action]")]
        public async Task<ActionResult> ConfirmEmail(string code)
        {
            if ( code == null)
            {
                return View("Error");
            }
            var sendToken = _protector.Unprotect(code);
            if (string.IsNullOrEmpty(sendToken))
            {
                return View("Error"); 
            }
            var userId = sendToken.Substring(0, sendToken.IndexOf('|'));
            var user = await _userManager.FindByIdAsync(userId);        
            var result = await _userManager.ConfirmEmailByProtectorAsync(_emailBindRepo, user, sendToken);
            return View(); 
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
