using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using Discussion.Admin.Models;
using Discussion.Admin.Supporting;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Discussion.Admin.Services.Impl
{
    public class AdminUserServiceImpl: IAdminUserService
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IPasswordHasher<AdminUser> _passwordHasher;

        public AdminUserServiceImpl(IOptions<JwtIssuerOptions> jwtOptions, 
            IPasswordHasher<AdminUser> passwordHasher)
        {
            _passwordHasher = passwordHasher;
            _jwtOptions = jwtOptions.Value;
        }

        
        public IssuedToken IssueJwtToken(AdminUser adminUser)
        {
            var bearerIdentity = new GenericIdentity(adminUser.Username, JwtBearerDefaults.AuthenticationScheme);
            var identity = new ClaimsIdentity(
                bearerIdentity,
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString(), ClaimValueTypes.Integer32),
                    new Claim(ClaimTypes.Name, adminUser.Username),

                    new Claim(JwtRegisteredClaimNames.Sub, adminUser.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, _jwtOptions.JtiGenerator()),
                    new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(),
                        ClaimValueTypes.Integer64),
                });
            
            var handler = new JwtSecurityTokenHandler();

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = _jwtOptions.SigningCredentials,
                NotBefore = _jwtOptions.NotBefore,
                Subject = identity,
                Expires = _jwtOptions.Expiration,
            });

            var encodedJwt = handler.WriteToken(securityToken);
            return new IssuedToken
            {
                TokenString = encodedJwt,
                ValidForSeconds = (int)_jwtOptions.ValidFor.TotalSeconds 
            };
        }

        public bool VerifyPassword(AdminUser adminUser, string providedPassword)
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

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        private static long ToUnixEpochDate(DateTime date)
            => new DateTimeOffset(date).ToUnixTimeSeconds();

    }
}