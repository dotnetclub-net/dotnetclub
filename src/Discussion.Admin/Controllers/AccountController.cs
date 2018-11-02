using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using Discussion.Admin.Services;
using Discussion.Admin.Supporting;
using Discussion.Admin.ViewModels;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
