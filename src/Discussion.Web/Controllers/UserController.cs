using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Utilities;
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
        
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        public UserController(UserManager<User> userManager, 
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        
        [Route("settings")]
        public IActionResult Settings()
        {
            return View(HttpContext.DiscussionUser());
        }
        
        [HttpPost]
        [Route("settings")]
        public async Task<IActionResult> DoSettings(EmailSettingViewModel emailSettingViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Settings");
            }

            var user = HttpContext.DiscussionUser();
            var existingEmail = user.EmailAddress?.Trim();
            var newEmail = emailSettingViewModel.EmailAddress?.Trim();

            if (existingEmail.IgnoreCaseEqual(newEmail))
            {
                return RedirectToAction("Settings");
            }
            
            var identityResult = await _userManager.SetEmailAsync(user, newEmail);
            if (identityResult.Succeeded)
            {
                return RedirectToAction("Settings");
            }
            
            var msg = string.Join(";", identityResult.Errors.Select(e => e.Description));
            ModelState.AddModelError(nameof(EmailSettingViewModel.EmailAddress), msg);
            return View("Settings");
        }

        [HttpPost]
        [Route("send-confirmation-mail")]
        public async Task<ApiResponse> SendEmailConfirmation(EmailSettingViewModel emailSettingViewModel)
        {
            var user = HttpContext.DiscussionUser();
            var tokenString = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenInEmail = new UserEmailToken {UserId = user.Id, Token = tokenString};

            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "User",
                new {token = tokenInEmail.EncodeAsUrlQueryString() },
                protocol: Request.Scheme);
            
            var sendBody = EmailExtensions.SplicedMailTemplate(callbackUrl);
            await _emailSender.SendEmailAsync(emailSettingViewModel.EmailAddress, "dotnet club 用户确认", sendBody);
            return  ApiResponse.NoContent();
        }

        [Route("confirm-email")]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string code)
        {
            var tokenInEmail = UserEmailToken.ExtractFromUrlQueryString(code);
            if (tokenInEmail == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(tokenInEmail.UserId.ToString());
            await _userManager.ConfirmEmailAsync(user, tokenInEmail.Token);
            return View(); 
        }
    }
}