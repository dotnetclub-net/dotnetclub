using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Utilities;
using Discussion.Web.Services.EmailConfirmation;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

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

            if (!existingEmail.IgnoreCaseEqual(newEmail))
            {
                await _userManager.SetEmailAsync(user, newEmail);
            }
            return RedirectToAction("Settings");
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

        class UserEmailToken
        {
            public int UserId { get; set; }
            public string Token { get; set; }
            
            
            public string EncodeAsUrlQueryString()
            {
                var callbackCode = $"userid={UserId}&token={WebUtility.UrlEncode(Token)}";
                return WebUtility.UrlEncode(Convert.ToBase64String(Encoding.ASCII.GetBytes(callbackCode)));
            }


            public static UserEmailToken ExtractFromUrlQueryString(string queryStringValue)
            {
                int userId = 0;
                string token = null;

                var query = QueryHelpers.ParseQuery(queryStringValue);
                var parsedSucceeded = query.TryGetValue("userid", out var userIdStr);
                parsedSucceeded = parsedSucceeded && int.TryParse(userIdStr, out userId);

                try
                {
                    parsedSucceeded = parsedSucceeded && query.TryGetValue("token", out var tokenString);
                    token = Encoding.ASCII.GetString(Convert.FromBase64String(tokenString));
                    parsedSucceeded = parsedSucceeded && !string.IsNullOrEmpty(token);
                }
                catch
                {
                    parsedSucceeded = false;
                }

                return parsedSucceeded
                    ? new UserEmailToken {UserId = userId, Token = token}
                    : null;
            }
        }
    }
}