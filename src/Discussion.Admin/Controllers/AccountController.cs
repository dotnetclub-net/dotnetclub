using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Discussion.Admin.Services;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Discussion.Admin.Controllers
{
    [Route("api/[controller]")]
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
        public object Signin([FromBody]UserViewModel model)
        {
            var adminUser = _adminUserRepo.All().SingleOrDefault(u => u.Username.ToLower() == model.UserName.ToLower());
            if (!_adminUserService.VerifyPassword(adminUser, model.Password))
            {
                ModelState.AddModelError("UserName", "用户名或密码错误");
            }

            if (!ModelState.IsValid)
            {
                return ApiResponse.Error(ModelState);
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
            var adminUserId = HttpContext.User
                .Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier)
                .Value;
            
            return Ok(new
            {
                Id = adminUserId,
                UserName = HttpContext.User.Identity.Name
            });
        }
    }
}
