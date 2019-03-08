﻿using System;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Time;
using Discussion.Web.Services.UserManagement.Exceptions;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Discussion.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IRepository<User> _userRepo;
        private readonly IClock _clock;
        private readonly SiteSettings _settings;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger, IRepository<User> userRepo, IClock clock, SiteSettings settings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _userRepo = userRepo;
            _clock = clock;
            _settings = settings;
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
                    lockoutOnFailure: false);

                var logLevel = result.Succeeded ? LogLevel.Information : LogLevel.Warning;
                var resultDesc = result.Succeeded ? "成功" : "失败";
                _logger.Log(logLevel, $"用户登录{resultDesc}：用户名 {viewModel.UserName}：{result}");
            }
            else
            {
                _logger.LogWarning($"用户登录失败：用户名 {viewModel.UserName}：数据格式不正确。");
            }

            if (!result.Succeeded)
            {
                ModelState.Clear();   // 将真正的验证结果隐藏掉（如果有的话）
                ModelState.AddModelError("UserName", "用户名或密码错误");
                return View("Signin");
            }

            var user = await _userManager.FindByNameAsync(viewModel.UserName);
            user.LastSeenAt = _clock.Now.UtcDateTime;
            _userRepo.Update(user);
            return RedirectTo(returnUrl);
        }

        [HttpPost]
        [Route("/signout")]
        [Authorize]
        public async Task<IActionResult> DoSignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectTo("/");
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
        public async Task<IActionResult> DoRegister(UserViewModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation($"用户注册失败：用户名 {registerModel.UserName}：数据格式不正确。");
                return View("Register");
            }

            if (!_settings.CanRegisterNewUsers())
            {
                const string errorMessage = "已关闭用户注册";
                _logger.LogWarning($"用户注册失败：用户名 {registerModel.UserName}：{errorMessage}");
                ModelState.AddModelError("UserName", errorMessage);
                return View("Register");
            }

            var newUser = new User
            {
                UserName = registerModel.UserName,
                DisplayName = registerModel.UserName,
                CreatedAtUtc = _clock.Now.UtcDateTime
            };

            var result = await _userManager.CreateAsync(newUser, registerModel.Password);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(";", result.Errors.Select(err => err.Description));
                ModelState.AddModelError("UserName", errorMessage);
                _logger.LogWarning($"用户注册失败：用户名 {registerModel.UserName}：{errorMessage}");
                return View("Register");
            }

            _logger.LogInformation($"新用户注册：用户名 {registerModel.UserName}");
            await _signInManager.PasswordSignInAsync(
                registerModel.UserName,
                registerModel.Password,
                isPersistent: false,
                lockoutOnFailure: true);
            return RedirectTo("/");
        }

        [Route("/retrieve-password")]
        public IActionResult RetrievePassword()
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectTo("/");
            }

            return View();
        }

        [HttpPost]
        [Route("/retrieve-password")]
        public ApiResponse DoRetrievePassword(RetrievePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"找回密码失败：用户名或邮箱 {model.UsernameOrEmail} 数据格式不正确。");
                return ApiResponse.Error(ModelState);
            }

            try
            {
                var user = GetUserBy(model);
                _logger.LogInformation($"找回密码：发送重置密码邮件到 {user.ConfirmedEmail} ");
                return ApiResponse.NoContent();
            }
            catch (RetrievePasswordVerificationException e)
            {
                _logger.LogWarning($"找回密码失败：{model.UsernameOrEmail} {e.Message}");
                return ApiResponse.Error(e.Message);
            }
        }

        User GetUserBy(RetrievePasswordModel model)
        {
            var usernameOrEmail = model.UsernameOrEmail.ToLower();

            var users = _userRepo
                .All()
                .Where(e => e.UserName.ToLower() == usernameOrEmail ||
                            e.EmailAddress != null && e.EmailAddress.ToLower() == usernameOrEmail)
                .ToList();

            if (!users.Any())
                throw new RetrievePasswordVerificationException("该用户不存在");

            var user = users.FirstOrDefault(e => e.EmailAddressConfirmed);

            if (user == null)
               throw new RetrievePasswordVerificationException("无法验证你对账号的所有权，因为之前没有已验证过的邮箱地址");

            return user;
        }

        IActionResult RedirectTo(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }
            return Redirect(returnUrl);
        }
    }
}
