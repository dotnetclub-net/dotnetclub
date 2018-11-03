using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using Discussion.Admin.Services;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Internal;

namespace Discussion.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IRepository<AdminUser> _adminUserRepo;
        private readonly IAdminUserService _adminUserService;

        public AccountController(IRepository<AdminUser> adminUserRepo, IAdminUserService adminUserService)
        {
            _adminUserRepo = adminUserRepo;
            _adminUserService = adminUserService;
        }

        [HttpPost("signin")]
        public object Signin(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.Error(ModelState);
            }
            
            var adminUser = _adminUserRepo.All().SingleOrDefault(u => u.Username.Equals(model.UserName, StringComparison.OrdinalIgnoreCase));
            if (!_adminUserService.VerifyPassword(adminUser, model.Password))
            {
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }

            var issuedToken = _adminUserService.IssueJwtToken(adminUser);
            return new
            {
                adminUser.Id,
                Token = issuedToken.TokenString,
                ExpiresInSeconds = issuedToken.ValidForSeconds
            };
        }

        [HttpPost("register")]
        public ApiResponse Register([FromBody]UserViewModel newAdminUser)
        {
            var isAuthenticated = HttpContext.IsAuthenticated();
            var canAccessAnonymously = !(_adminUserRepo.All().Any());
            if (!canAccessAnonymously && !isAuthenticated)
            {
                return ApiResponse.NoContent(HttpStatusCode.Unauthorized);
            }
    
            var userNameTaken =_adminUserRepo.All().Any(u => u.Username.ToLower() == newAdminUser.UserName.ToLower());
            if (userNameTaken)
            {
                ModelState.AddModelError(nameof(UserViewModel.UserName), "用户名已被占用");
            }
            
            if (!ModelState.IsValid)
            {
                return ApiResponse.Error(ModelState);
            }
            
            var admin = new AdminUser
            {
                Username = newAdminUser.UserName,
                HashedPassword = _adminUserService.HashPassword(newAdminUser.Password)
            };
            _adminUserRepo.Save(admin);
    
            return ApiResponse.NoContent();
        }

        [HttpGet("user")]
        [Authorize]
        public IActionResult UserInfo()
        {
            return Ok(new
            {
                status = 1,
                result = new
                {
                    UserName = "dotnet_lover",
                }
            });
        }
    }
}
