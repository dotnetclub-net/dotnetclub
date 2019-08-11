using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Time;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;

namespace Discussion.Web.Services.UserManagement
{
    public class ExternalSigninManager
    {
        private readonly ILogger<ExternalSigninManager> _logger;
        private readonly ExternalIdentityServiceOptions _idpOptions;
        private readonly SiteSettings _siteSettings;
        private readonly IClock _clock;
        private readonly UserManager<User> _userManager;
        private readonly IUserClaimsPrincipalFactory<User> _principalFactory;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<VerifiedPhoneNumber> _phoneNumberVerificationRepo;
        private readonly IRepository<SessionRevocationRecord> _sessionRevocationRepo;

        public ExternalSigninManager(ILogger<ExternalSigninManager> logger, 
            IRepository<User> userRepo, IOptions<ExternalIdentityServiceOptions> idpOptions, 
            SiteSettings siteSettings, IRepository<VerifiedPhoneNumber> phoneNumberVerificationRepo, 
            UserManager<User> userManager, IUserClaimsPrincipalFactory<User> principalFactory, IClock clock, IRepository<SessionRevocationRecord> sessionRevocationRepo)
        {
            _logger = logger;
            _userRepo = userRepo;
            _idpOptions = idpOptions.Value;
            _siteSettings = siteSettings;
            _phoneNumberVerificationRepo = phoneNumberVerificationRepo;
            _userManager = userManager;
            _principalFactory = principalFactory;
            _clock = clock;
            _sessionRevocationRepo = sessionRevocationRepo;
        }
        
        
        public async Task TrySignOutLocalSession(RemoteSignOutContext context)
        {
            if (!string.Equals(context.Request.Method, HttpMethod.Post.ToString(), StringComparison.OrdinalIgnoreCase)
                || context.Request.ContentLength == null || context.Request.ContentLength.Value <= 0)
            {
                return;
            }

            var backchannelLogoutInvocation = await new StreamReader(context.Request.Body).ReadToEndAsync();
            if (!context.Options.SecurityTokenValidator.CanReadToken(backchannelLogoutInvocation))
            {
                return;
            }

            var idpServerConfiguration = await context.Options.ConfigurationManager.GetConfigurationAsync(CancellationToken.None);
            var claimsPrincipal = context.Options.SecurityTokenValidator.ValidateToken(
                backchannelLogoutInvocation,
                new TokenValidationParameters()
                {
                    IssuerSigningKeys = idpServerConfiguration.SigningKeys,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false
                }, out _);

            var sessionId = claimsPrincipal.FindFirst(c => c.Type == "adapterSessionIds")?.Value;
            if (sessionId != null)
            {
                _sessionRevocationRepo.Save(new SessionRevocationRecord()
                {
                    SessionId = sessionId,
                    Reason = "用户从外部身份服务处登出"
                });
                context.HandleResponse();
            }
        }

        public async Task<ClaimsPrincipal> TransformToDiscussionUser(HttpContext httpContext, IList<Claim> claims)
        {
            var userIdClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject) 
                              ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                var errorMessage = "无法从外部身份服务的回调中取得 Subject 或 NameIdentifier 身份声明的值";
                _logger.LogWarning("用户登录失败：{@LoginAttempt}", new {UserName = string.Empty, Result = errorMessage});
                throw new InvalidOperationException(errorMessage);
            }

            var user = _userRepo.All().FirstOrDefault(u => u.OpenIdProvider == _idpOptions.ProviderId && u.OpenId == userIdClaim.Value);
            if (user == null)
            {
                user = await ImportNewUser(claims, userIdClaim);
            }
            else
            {
                user.LastSeenAt = _clock.Now.UtcDateTime;
                _userRepo.Update(user);
                _logger.LogInformation("用户登录成功：{@RegisterAttempt}", new {user.UserName, Result = $"从外部身份服务 {_idpOptions.ProviderId} 登录成功"});
            }

            var externalTokenId = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.JwtId)?.Value;
            if (externalTokenId != null)
            {
                httpContext.Items["SessionId"] = externalTokenId;
            }

            return await _principalFactory.CreateAsync(user);
        }

        public async Task<User> ImportNewUser(IList<Claim> claims, Claim userIdClaim)
        {
            var originalUserName = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.PreferredUserName)?.Value ?? userIdClaim.Value;
            var userName = string.Concat(originalUserName , "@", _idpOptions.ProviderId);
            if (!_siteSettings.CanRegisterNewUsers())
            {
                const string errorMessage = "已关闭用户注册";
                _logger.LogWarning("用户注册失败：{@RegisterAttempt}", new {username = userName, Result = errorMessage});
                throw new InvalidOperationException(errorMessage);
            }

            var displayNameClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value
                                    ?? claims.FirstOrDefault(x => x.Type == JwtClaimTypes.NickName)?.Value;
            var emailClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value;
            var emailVerifiedClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.EmailVerified)?.Value;
            var emailVerified = false;
            if (!string.IsNullOrEmpty(emailClaim) && Boolean.TryParse(emailVerifiedClaim, out emailVerified))
            {
                // nothing to do...
            }

            var phoneNumberClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.PhoneNumber)?.Value;
            VerifiedPhoneNumber verifiedPhoneNumber = null;
            var phoneNumberVerifiedClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.PhoneNumberVerified)?.Value;
            if (!string.IsNullOrEmpty(phoneNumberClaim) &&
                Boolean.TryParse(phoneNumberVerifiedClaim, out var phoneNumberVerified) && phoneNumberVerified)
            {
                verifiedPhoneNumber = new VerifiedPhoneNumber()
                {
                    PhoneNumber = phoneNumberClaim
                };
                _phoneNumberVerificationRepo.Save(verifiedPhoneNumber);
            }

            var user = new User
            {
                UserName = userName,
                DisplayName = string.IsNullOrWhiteSpace(displayNameClaim) ? originalUserName : displayNameClaim,
                CreatedAtUtc = _clock.Now.UtcDateTime,
                EmailAddress = emailClaim,
                EmailAddressConfirmed = emailVerified,
                OpenId = userIdClaim.Value,
                OpenIdProvider = _idpOptions.ProviderId,
                LastSeenAt = _clock.Now.UtcDateTime,
                PhoneNumberId = verifiedPhoneNumber?.Id
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(";", result.Errors.Select(err => err.Description ?? err.Code));
                _logger.LogWarning("用户注册失败：{@LoginAttempt}", new {UserName = userName, Result = errorMessage});
                throw new InvalidOperationException(errorMessage);
            }

            _logger.LogInformation("用户注册成功：{@RegisterAttempt}", new {UserName = userName, UserId = user.Id});
            return user;
        }
    }
}