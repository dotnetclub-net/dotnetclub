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

namespace Discussion.Web.Controllers
{
    
    [Route("user")]
    [Authorize]
    public class UserController: Controller
    {
        public const string ConfigKeyRequireUserPhoneNumberVerified = "RequireUserPhoneNumberVerified"; 
        
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;

        public UserController(UserManager<User> userManager, IUserService userService)
        {
            _userManager = userManager;
            _userService = userService;
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
            var user = HttpContext.DiscussionUser();
            if (!ModelState.IsValid)
            {
                return View("Settings", user);
            }

            var identityResult = await _userService.UpdateUserInfoAsync(user, userSettingsViewModel);
            if (identityResult.Succeeded)
            {
                return RedirectToAction("Settings");
            }

            var msg = string.Join(";", identityResult.Errors.Select(e => e.Description));
            ModelState.AddModelError(string.Empty, msg);
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
                return  ApiResponse.NoContent();
            }
            catch (UserEmailAlreadyConfirmedException ex)
            {
                return ApiResponse.Error(ex.Message);
            }

        }


        [HttpGet]
        [Route("confirm-email")]
        [AllowAnonymous]
        public async Task<ViewResult> ConfirmEmail(string token)
        {
            var currentUser = HttpContext.DiscussionUser();
            var tokenInEmail = token == null ? null : UserEmailToken.ExtractFromUrlQueryString(token);
            if (tokenInEmail == null || tokenInEmail.UserId != currentUser.Id)
            {
                return View(false);
            }

            var result = await _userService.ConfirmEmailAsync(currentUser, tokenInEmail);
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
            if (!ModelState.IsValid)
            {
                return View(getActionName, viewModel);
            }
            
            var user = HttpContext.DiscussionUser();
            var changeResult = await _userManager.ChangePasswordAsync(user, viewModel.OldPassword, viewModel.NewPassword);
            if (!changeResult.Succeeded)
            {
                ModelState.Clear();
                changeResult.Errors.ToList().ForEach(err =>
                {
                    ModelState.AddModelError(err.Code, err.Description);    
                });
                return View(getActionName, viewModel);
            }

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
            var badRequestResponse = ApiResponse.NoContent(HttpStatusCode.BadRequest);
            if (string.IsNullOrEmpty(phoneNumber) || !Regex.IsMatch(phoneNumber, "\\d{11}"))
            {
                return badRequestResponse;
            }

            try
            {
                var user = HttpContext.DiscussionUser();
                await _userService.SendPhoneNumberVerificationCodeAsync(user, phoneNumber);
            }
            catch(PhoneNumberVerificationFrequencyExceededException)
            {
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
            }
            catch (PhoneNumberVerificationCodeInvalidException)
            {
                return ApiResponse.Error("code", "验证码不正确或已过期");
            }
            
            return ApiResponse.NoContent();
        }
    }
}