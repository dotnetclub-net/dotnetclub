using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Discussion.Admin.Services;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Discussion.Admin.Tests.Specs.Services
{
    [Collection("AdminSpecs")]
    public class AdminUserServiceSpecs
    {
        private readonly TestDiscussionAdminApp _adminApp;

        public AdminUserServiceSpecs(TestDiscussionAdminApp adminApp)
        {
            _adminApp = adminApp;
        }

        [Fact]
        public void should_generate_token_with_7200_validity()
        {
            var adminUserService = _adminApp.GetService<IAdminUserService>();
            var adminUser = new AdminUser
            {
                Id = 20,
                Username = "someone"
            };

            var token = adminUserService.IssueJwtToken(adminUser);
            Assert.NotNull(token);
            Assert.NotNull(token.TokenString);
            Assert.Equal(7200, token.ValidForSeconds);
        }

        [Fact]
        public void should_generate_valid_token_with_correct_identity()
        {
            var adminUserService = _adminApp.GetService<IAdminUserService>();
            var adminUser = new AdminUser
            {
                Id = 33,
                Username = "mike"
            };

            var token = adminUserService.IssueJwtToken(adminUser);

            var (tokenValidator, validationParameters) = GetJwtTokenValidator();
            var claimsPrincipal = tokenValidator.ValidateToken(token.TokenString, validationParameters, out _);
            var identity = claimsPrincipal.Identities.First();

            Assert.NotNull(claimsPrincipal);
            Assert.Equal(1, claimsPrincipal.Identities.Count());
            Assert.Equal("33", identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal("mike", identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
        }

        [Fact]
        public void should_generate_valid_token_with_correct_jwt_token_information()
        {
            var adminUserService = _adminApp.GetService<IAdminUserService>();
            var adminUser = new AdminUser
            {
                Id = 576,
                Username = "david"
            };

            var token = adminUserService.IssueJwtToken(adminUser);

            var (tokenValidator, validationParameters) = GetJwtTokenValidator();
            var claimsPrincipal = tokenValidator.ValidateToken(token.TokenString, validationParameters, out var securityToken);
            var jwtToken = securityToken as JwtSecurityToken;
            

            Assert.NotNull(claimsPrincipal);
            Assert.NotNull(jwtToken);
            Assert.Equal(TestDiscussionAdminApp.JwtIssuer, jwtToken.Issuer);
            Assert.Contains(TestDiscussionAdminApp.JwtAudience, jwtToken.Audiences);
            
            Assert.Equal("david", jwtToken.Subject);
            Assert.Equal(7200, (securityToken.ValidTo - securityToken.ValidFrom).TotalSeconds);
        }

        
        
        
        
        private (ISecurityTokenValidator, TokenValidationParameters) GetJwtTokenValidator()
        {
            var options = _adminApp.GetService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer");
            
            var tokenValidator = options.SecurityTokenValidators.First();
            return (tokenValidator, options.TokenValidationParameters);
        }
    }
}