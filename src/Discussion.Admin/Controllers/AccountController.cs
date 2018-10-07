using System;
using System.Security.Claims;
using System.Security.Principal;
using Discussion.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Discussion.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtIssuerOptions jwtOptions;

        public AccountController(IOptions<JwtIssuerOptions> jwtOptions)
        {
            this.jwtOptions = jwtOptions.Value;
        }

        [HttpPost("signin")]
        public IActionResult Signin(SigninModel model)
        {
            var identity = new ClaimsIdentity(new GenericIdentity(model.UserName, "Token"), new[]
            {
                new Claim("id","123" ),
                new Claim("rol", "api_access"),
                new Claim(ClaimTypes.Name, model.UserName),

                new Claim(JwtRegisteredClaimNames.Sub, model.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,  jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
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

            return Ok(new
            {
                status = 1,
                result = new
                {
                    id = identity.Claims.Single(c => c.Type == "id").Value,
                    auth_token = encodedJwt,
                    expires_in = (int)jwtOptions.ValidFor.TotalSeconds
                }
            });
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

        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}