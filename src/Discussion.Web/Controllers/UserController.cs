using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Utilities;
using Discussion.Web.Services;
using Discussion.Web.Services.EmailConfirmation;
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
        private readonly IEmailSender _emailSender;
        private readonly IConfirmationEmailBuilder _emailBuilder;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<PhoneNumberVerificationRecord> _verificationCodeRecordRepo;
        private readonly IRepository<VerifiedPhoneNumber> _phoneNumberRepo;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISmsSender _smsSender;

        public UserController(UserManager<User> userManager, IEmailSender emailSender,
            IConfirmationEmailBuilder emailBuilder, IRepository<User> userRepo, ISmsSender smsSender,
            IRepository<PhoneNumberVerificationRecord> verificationCodeRecordRepo,
            ApplicationDbContext dbContext, IRepository<VerifiedPhoneNumber> phoneNumberRepo)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailBuilder = emailBuilder;
            _userRepo = userRepo;
            _smsSender = smsSender;
            _verificationCodeRecordRepo = verificationCodeRecordRepo;
            _dbContext = dbContext;
            _phoneNumberRepo = phoneNumberRepo;
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
            var emailFieldName = nameof(UserSettingsViewModel.EmailAddress);
            if (!ModelState.IsValid)
            {
                return View("Settings", user);
            }

            user.AvatarFileId = userSettingsViewModel.AvatarFileId;
            user.DisplayName = userSettingsViewModel.DisplayName;
            if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                user.DisplayName = user.UserName;
            }
            _userRepo.Update(user);
            
            var existingEmail = user.EmailAddress?.Trim();
            var newEmail = userSettingsViewModel.EmailAddress?.Trim();
            if (existingEmail.IgnoreCaseEqual(newEmail))
            {
                return RedirectToAction("Settings");
            }
            
            var emailTaken = IsEmailTaken(user.Id, newEmail);
            if (emailTaken)
            {
                ModelState.AddModelError(emailFieldName, "邮件地址已由其他用户使用");
                return View("Settings", user);
            }
            
            var identityResult = await _userManager.SetEmailAsync(user, newEmail);
            if (!identityResult.Succeeded)
            {
                var msg = string.Join(";", identityResult.Errors.Select(e => e.Description));
                ModelState.AddModelError(emailFieldName, msg);
                return View("Settings", user);
            }
            
            return RedirectToAction("Settings");
        }

        [HttpPost]
        [Route("send-confirmation-mail")]
        public async Task<ApiResponse> SendEmailConfirmation()
        {
            var user = HttpContext.DiscussionUser();
            if (user.EmailAddressConfirmed)
            {
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }
            
            var tokenString = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenInEmail = new UserEmailToken {UserId = user.Id, Token = tokenString};

            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "User",
                new { token = tokenInEmail.EncodeAsUrlQueryString() },
                protocol: Request.Scheme);
            
            var emailBody = _emailBuilder.BuildEmailBody(callbackUrl);
            await _emailSender.SendEmailAsync(user.EmailAddress, "dotnet club 用户邮件地址确认", emailBody);
            return  ApiResponse.NoContent();
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
                if (!hasErrors && IsEmailTaken(user.Id, user.EmailAddress))
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

        private bool IsEmailTaken(int userId, string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
            {
                return false;
            }
            
            return _userRepo.All()
                .Any(u => u.EmailAddressConfirmed
                          && u.Id != userId
                          && u.EmailAddress != null
                          && u.EmailAddress.ToLower() == newEmail.ToLower());
        }
    }
}