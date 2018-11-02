using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
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
        private readonly JwtIssuerOptions jwtOptions;
        private readonly IRepository<AdminUser> _adminUserRepo;
        private readonly IPasswordHasher<AdminUser> _passwordHasher;

        public AccountController(IOptions<JwtIssuerOptions> jwtOptions, IRepository<AdminUser> adminUserRepo, IPasswordHasher<AdminUser> passwordHasher)
        {
            _adminUserRepo = adminUserRepo;
            _passwordHasher = passwordHasher;
            this.jwtOptions = jwtOptions.Value;
        }

        [HttpPost("signin")]
        public object Signin(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.Error(ModelState);
            }
            
            var adminUser = _adminUserRepo.All().SingleOrDefault(u => u.Username.Equals(model.UserName, StringComparison.OrdinalIgnoreCase));
            if (!VerifyPassword(adminUser, model.Password))
            {
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }

            var bearerIdentity = new GenericIdentity(model.UserName, JwtBearerDefaults.AuthenticationScheme);
            var identity = new ClaimsIdentity(
                bearerIdentity,
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, model.UserName),

                    new Claim(JwtRegisteredClaimNames.Sub, model.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtOptions.JtiGenerator()),
                    new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(jwtOptions.IssuedAt).ToString(),
                        ClaimValueTypes.Integer64),
                });

            var handler = new JwtSecurityTokenHandler();

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audience,
                SigningCredentials = jwtOptions.SigningCredentials,
                NotBefore = jwtOptions.NotBefore,
                Subject = identity,
                Expires = jwtOptions.Expiration,
            });

            var encodedJwt = handler.WriteToken(securityToken);

            return new
            {
                adminUser.Id,
                Token = encodedJwt,
                Expires = (int) jwtOptions.ValidFor.TotalSeconds
            };
        }

        private bool VerifyPassword(AdminUser adminUser, string providedPassword)
        {
            if (adminUser == null)
            {
                return false;
            }
            
            var pwdVerification = _passwordHasher.VerifyHashedPassword(
                                                    adminUser, 
                                                    adminUser.HashedPassword, 
                                                    providedPassword);
            return pwdVerification == PasswordVerificationResult.Success || 
                   pwdVerification == PasswordVerificationResult.SuccessRehashNeeded;
        }
        
        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

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
                HashedPassword = _passwordHasher.HashPassword(null, newAdminUser.Password)
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
