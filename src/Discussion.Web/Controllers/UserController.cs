using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Web.Services.UserManagement;
using Discussion.Web.Services.UserManagement.Exceptions;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Discussion.Core.Logging;

namespace Discussion.Web.Controllers
{
    
    [Route("user")]
    [Authorize]
    public class UserController: Controller
    {
        public const string ConfigKeyRequireUserPhoneNumberVerified = "RequireUserPhoneNumberVerified"; 
        
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<User> userManager, IUserService userService, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
        }

        
        [Route("settings")]
        public IActionResult Settings()
        {
            return View(HttpContext.DiscussionUser());
        }
        
        [HttpPost]
        [Route("settings")]
        public async Task<IActionResult> DoSettings(UserSettingsViewModel userSettingsViewModel)
        {
            const string action = "更新个人信息";
            var user = HttpContext.DiscussionUser();
            if (!ModelState.IsValid)
            {
                _logger.LogModelState(action, ModelState, user.UserName);
                return View("Settings", user);
            }

            var identityResult = await _userService.UpdateUserInfoAsync(user, userSettingsViewModel);
            if (identityResult.Succeeded)
            {
                _logger.LogIdentityResult(action, identityResult, user.UserName);
                return RedirectToAction("Settings");
            }

            var msg = string.Join(";", identityResult.Errors.Select(e => e.Description));
            ModelState.AddModelError(string.Empty, msg);
            _logger.LogIdentityResult(action, identityResult, user.UserName);
            return View("Settings", user);
        }


        [HttpPost]
        [Route("send-confirmation-mail")]
        public async Task<ApiResponse> SendEmailConfirmation()
        {
            try
            {
                var user = HttpContext.DiscussionUser();
                await _userService.SendEmailConfirmationMailAsync(user, Request.Scheme);
                _logger.LogInformation($"发送确认邮件成功：{user.UserName}");
                return  ApiResponse.NoContent();
            }
            catch (UserEmailAlreadyConfirmedException ex)
            {
                _logger.LogWarning($"发送确认邮件失败：{ex.UserName}：{ex.Message}");
                return ApiResponse.Error(ex.Message);
            }
        }


        [HttpGet]
        [Route("confirm-email")]
        [AllowAnonymous]
        public async Task<ViewResult> ConfirmEmail(string token)
        {
            var tokenInEmail = token == null ? null : UserEmailToken.ExtractFromUrlQueryString(token);
            if (tokenInEmail == null)
            {
                _logger.LogWarning("确认邮件地址失败：无法识别提供的 token");
                return View(false);
            }

            var result = await _userService.ConfirmEmailAsync(tokenInEmail);
            _logger.LogIdentityResult("确认邮件地址", result);
            return View(result.Succeeded); 
        }

        [Route("change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<IActionResult> DoChangePassword(ChangePasswordViewModel viewModel)
        {
            var getActionName = "ChangePassword";
            var user = HttpContext.DiscussionUser();
            if (!ModelState.IsValid)
            {
                _logger.LogModelState("修改密码", ModelState, user.UserName);
                return View(getActionName, viewModel);
            }
            
            var changeResult = await _userManager.ChangePasswordAsync(user, viewModel.OldPassword, viewModel.NewPassword);
            if (!changeResult.Succeeded)
            {
                ModelState.Clear();
                changeResult.Errors.ToList().ForEach(err =>
                {
                    ModelState.AddModelError(err.Code, err.Description);    
                });
                _logger.LogIdentityResult("修改密码", changeResult, user.UserName);
                return View(getActionName, viewModel);
            }

            _logger.LogIdentityResult("修改密码", changeResult, user.UserName);
            return RedirectToAction(getActionName);
        }


        [Route("phone-number-verification")]
        public IActionResult VerifyPhoneNumber([FromForm] string code)
        {
            return View();
        }

        [HttpPost]
        [Route("phone-number-verification/send-code")]
        public async Task<ApiResponse> SendPhoneNumberVerificationCode([FromForm] string phoneNumber)
        {
            var user = HttpContext.DiscussionUser();
            var badRequestResponse = ApiResponse.NoContent(HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(phoneNumber) || !Regex.IsMatch(phoneNumber, "\\d{11}"))
            {
                _logger.LogWarning($"发送手机验证短信失败：{user.UserName}：号码 {phoneNumber} 不正确");
                return badRequestResponse;
            }

            try
            {
                await _userService.SendPhoneNumberVerificationCodeAsync(user, phoneNumber);
                _logger.LogInformation($"发送手机验证短信成功：{user.UserName}");
            }
            catch(PhoneNumberVerificationFrequencyExceededException)
            {
                _logger.LogWarning($"发送手机验证短信失败：{user.UserName}：超出调用限制");
                return badRequestResponse;                
            }
                
            return ApiResponse.NoContent();
        }

        [HttpPost]
        [Route("phone-number-verification/verify")]
        public ApiResponse DoVerifyPhoneNumber([FromForm] string code)
        {
            var user = HttpContext.DiscussionUser();

            try
            {
                _userService.VerifyPhoneNumberByCode(user, code);
                _logger.LogInformation($"验证手机号码成功：{user.UserName}");
            }
            catch (PhoneNumberVerificationCodeInvalidException)
            {
                _logger.LogWarning($"验证手机号码失败：{user.UserName}：验证码不正确或已过期");
                return ApiResponse.Error("code", "验证码不正确或已过期");
            }
            
            return ApiResponse.NoContent();
        }


    }
}