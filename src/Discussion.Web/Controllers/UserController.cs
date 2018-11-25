using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discussion.Core.Communication.Email;
using Discussion.Core.Communication.Sms;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Utilities;
using Discussion.Web.Services;
using Discussion.Web.Services.UserManagement;
using Discussion.Web.Services.UserManagement.EmailConfirmation;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Controllers
{
    // ReSharper disable Mvc.ActionNotResolved
    // ReSharper disable Mvc.ControllerNotResolved
    
    [Route("user")]
    [Authorize]
    public class UserController: Controller
    {
        public const string ConfigKeyRequireUserPhoneNumberVerified = "RequireUserPhoneNumberVerified"; 
        
        private readonly UserManager<User> _userManager;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<PhoneNumberVerificationRecord> _verificationCodeRecordRepo;
        private readonly IRepository<VerifiedPhoneNumber> _phoneNumberRepo;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISmsSender _smsSender;
        private readonly IUserService _userService;

        public UserController(UserManager<User> userManager, 
            IRepository<User> userRepo, ISmsSender smsSender,
            IRepository<PhoneNumberVerificationRecord> verificationCodeRecordRepo,
            ApplicationDbContext dbContext, IRepository<VerifiedPhoneNumber> phoneNumberRepo, IUserService userService)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _smsSender = smsSender;
            _verificationCodeRecordRepo = verificationCodeRecordRepo;
            _dbContext = dbContext;
            _phoneNumberRepo = phoneNumberRepo;
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

            var identityResult = await _userService.UpdateUserInfo(userSettingsViewModel, user);
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
                await _userService.SendEmailConfirmationMail(user, Request.Scheme);
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
        public async Task<ActionResult> ConfirmEmail(string token)
        {
            var tokenInEmail = token == null ? null : UserEmailToken.ExtractFromUrlQueryString(token);
            var hasErrors = tokenInEmail == null;
            if (!hasErrors)
            {
                var user = await _userManager.FindByIdAsync(tokenInEmail.UserId.ToString());
                var identityResult = await _userManager.ConfirmEmailAsync(user, tokenInEmail.Token);
                hasErrors = !identityResult.Succeeded;
                if (!hasErrors && _userService.IsEmailTakenOtherUser(user.Id, user.EmailAddress))
                {
                    hasErrors = true;
                    user.EmailAddressConfirmed = false;
                    await _userManager.UpdateAsync(user);
                }
            }

            if (hasErrors)
            {
                ModelState.AddModelError("token", "无法确认邮件地址");
            }
            return View(); 
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
                ModelState.AddModelError(string.Empty, changeResult.Errors.ToList().First().Description);
                return View(getActionName, viewModel);
            }

            return RedirectToAction(getActionName);
        }


        [Route("phone-number-verification")]
        public IActionResult VerifyPhoneNumber([FromForm] string code)
        {
            var user = HttpContext.DiscussionUser();
            _dbContext.Entry(user).Reference(u => u.VerifiedPhoneNumber).Load();
            return View(user);
        }

        [HttpPost]
        [Route("phone-number-verification/send-code")]
        public async Task<ApiResponse> SendPhoneNumberVerificationCode([FromForm] string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || !Regex.IsMatch(phoneNumber, "\\d{11}"))
            {
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }
            
            var user = HttpContext.DiscussionUser();
            _dbContext.Entry(user).Reference(u => u.VerifiedPhoneNumber).Load();
            var today = DateTime.UtcNow.Date;

            var msgSentToday = _verificationCodeRecordRepo
                .All()
                .Where(r => r.UserId == user.Id && r.CreatedAtUtc >= today)
                .ToList();
            var sentInTwoMinutes = msgSentToday.Any(r => r.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-2));
            
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var verifiedNotLongerThan7Days = user.VerifiedPhoneNumber != null && (user.VerifiedPhoneNumber.CreatedAtUtc > sevenDaysAgo || user.VerifiedPhoneNumber.CreatedAtUtc > sevenDaysAgo);
            if (msgSentToday.Count >= 5 || sentInTwoMinutes || verifiedNotLongerThan7Days)
            {
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }
            
            var verificationCode = StringUtility.RandomNumbers(length:6);
            var record = new PhoneNumberVerificationRecord
            {
                // ReSharper disable PossibleInvalidOperationException
                UserId = User.ExtractUserId().Value,
                Code = verificationCode,
                Expires = DateTime.UtcNow.AddMinutes(15),
                PhoneNumber = phoneNumber
            };
            _verificationCodeRecordRepo.Save(record);
            await _smsSender.SendVerificationCodeAsync(phoneNumber, verificationCode);
            return ApiResponse.NoContent();
        }

        [HttpPost]
        [Route("phone-number-verification/verify")]
        public ApiResponse DoVerifyPhoneNumber([FromForm] string code)
        {
            var user = HttpContext.DiscussionUser();
            var validCode = _verificationCodeRecordRepo
                .All()
                .FirstOrDefault(r => r.UserId == user.Id && r.Code == code && r.Expires > DateTime.UtcNow);

            if (validCode == null)
            {
                return ApiResponse.Error("code", "验证码不正确或已过期");
            }

            _dbContext.Entry(user).Reference(u => u.VerifiedPhoneNumber).Load();
            if (user.VerifiedPhoneNumber == null)
            {
                user.VerifiedPhoneNumber = new VerifiedPhoneNumber{ PhoneNumber =  validCode.PhoneNumber };
                _phoneNumberRepo.Save(user.VerifiedPhoneNumber);
            }
            else
            {
                user.VerifiedPhoneNumber.PhoneNumber = validCode.PhoneNumber;
                user.VerifiedPhoneNumber.ModifiedAtUtc = DateTime.UtcNow;
                _phoneNumberRepo.Update(user.VerifiedPhoneNumber);
            }
            _userRepo.Update(user);
            
            return ApiResponse.NoContent();
        }
    }
}