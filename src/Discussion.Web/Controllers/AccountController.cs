using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Web.Services.EmailConfirmation;
using Discussion.Core.Mvc;
using Discussion.Core.ViewModels;
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
using Microsoft.AspNetCore.Authorization;

namespace Discussion.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IRepository<EmailBindOptions> _emailBindRepo;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IDataProtector _protector;

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
        public async Task<IActionResult> DoSignin([FromForm]UserViewModel viewModel, [FromQuery]string returnUrl)
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
        public async Task<IActionResult> DoRegister(UserViewModel userViewModel)
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

        [Authorize]
        [Route("/setting")]
        public IActionResult Setting()
        {
            return View(HttpContext.DiscussionUser());
        }
        
        [HttpPost]
        [Authorize]
        [Route("/setting")]
        public async Task<IActionResult> DoSetting(EmailSettingViewModel emailSettingViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Setting");
            }
            
            var user = HttpContext.DiscussionUser();  
            var newEmailAddress = emailSettingViewModel.EmailAddress;
            if (!IsEmailAddressNeverUsed(user.EmailAddress, newEmailAddress, out var usedEmailAddress))
            {
                ModelState.AddModelError("EmailAddress", "请不要使用曾绑定过的邮箱");
                return View("Setting");
            }

            user.EmailAddress = emailSettingViewModel.EmailAddress;
            await _userManager.UpdateAsync(user);
            
            
            var validationToken = $"{user.Id.ToString()}|dotnetclub";
            _emailBindRepo.Save(new EmailBindOptions
            {
                UserId = user.Id,
                EmailAddress = emailSettingViewModel.EmailAddress,
                OldEmailAddress = usedEmailAddress,
                CallbackToken = validationToken,
                IsActivated = false,
                CreatedAtUtc = DateTime.Now
            });

            var sendToken = _protector.Protect(validationToken);
            var callBack = Url.Action(
                action: "ConfirmEmail",
                controller: "Account",
                values: new { code = sendToken },
                protocol: Request.Scheme);
            var sendBody = EmailExtensions.SplicedMailTemplate(HtmlEncoder.Default.Encode(callBack));
            await _emailSender.SendEmailAsync(emailSettingViewModel.EmailAddress, "dotnetclub用户确认", sendBody);
            
            return RedirectToAction("Setting");
        }
        
        [Route("confirm-email")]
        public async Task<ActionResult> ConfirmEmail(string code)
        {
            var protectedToken = ExtractProtectedToken(code);
            if (string.IsNullOrEmpty(protectedToken))
            {
                return BadRequest();
            }
            
            var userId = protectedToken.Substring(0, protectedToken.IndexOf('|'));
            var user = await _userManager.FindByIdAsync(userId);        
            
            await _userManager.ConfirmEmailByProtectorAsync(_emailBindRepo, user, protectedToken);
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


        private string ExtractProtectedToken(string code)
        {
            return code == null ? null : _protector.Unprotect(code);
        }

        private bool IsEmailAddressNeverUsed(string userEmail, string newEmailAddress, out string usedEmailAddress)
        {
            usedEmailAddress = null;
            if (string.IsNullOrWhiteSpace(userEmail))
                return true;
            
            var emailBindRecord = _emailBindRepo
                .All()
                .SingleOrDefault(t => t.EmailAddress.ToLower() == userEmail.ToLower());
            usedEmailAddress = emailBindRecord?.OldEmailAddress;
            
            if (usedEmailAddress != null 
                && usedEmailAddress.Equals(newEmailAddress, StringComparison.OrdinalIgnoreCase))
                return false;
            else
                usedEmailAddress = userEmail;
            return true;
        }
    }
}
