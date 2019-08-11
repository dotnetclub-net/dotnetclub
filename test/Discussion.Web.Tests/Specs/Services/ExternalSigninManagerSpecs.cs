using System;
using System.Collections.Generic;
using System.Security.Claims;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Web.Services;
using Discussion.Web.Services.UserManagement;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Linq;
using Discussion.Tests.Common.AssertionExtensions;

namespace Discussion.Web.Tests.Specs.Services
{
    [Collection("WebSpecs")]
    public class ExternalSigninManagerSpecs
    {
        private readonly TestDiscussionWebApp _app;

        public ExternalSigninManagerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
            app.DeleteAll<User>();
        }

        [Fact]
        async void should_import_new_user_and_signin_from_external_idp()
        {
            var userId = StringUtility.Random(5);
            var name = StringUtility.Random();
            var emailAddress = $"{StringUtility.Random(5)}@someplace.com";
            MockExternalSignin(false, out var httpContext);

            var importedPrincipal = await _app.GetService<ExternalSigninManager>()
                .TransformToDiscussionUser(httpContext,
                    ComposeJwtClaims(userId, name, emailAddress, Guid.NewGuid().ToString("D")));

            var userRepo = _app.GetService<IRepository<User>>();
            var importedUser = userRepo.All().FirstOrDefault(user => user.OpenId == userId && user.OpenIdProvider == "external");
            
            VerifyTransformedUser(importedUser, importedPrincipal, name, emailAddress, true);
        }

        [Fact]
        async void should_signin_existing_user_without_latest_claims_using_external_idp_enabled()
        {
            var userId = StringUtility.Random(5);
            var name = StringUtility.Random();
            var emailAddress = $"{StringUtility.Random(5)}@someplace.com";

            var existingUser = _app.CreateUser(userId, displayName: name);
            existingUser.OpenId = userId;
            existingUser.EmailAddress = emailAddress;
            existingUser.OpenIdProvider = "external";
            var userRepo = _app.GetService<IRepository<User>>();
            userRepo.Update(existingUser);
            
            MockExternalSignin(false, out var httpContext);
            
            var importedPrincipal = await _app.GetService<ExternalSigninManager>()
                .TransformToDiscussionUser(httpContext,
                    ComposeJwtClaims(userId, name, emailAddress, Guid.NewGuid().ToString("D")));

            VerifyTransformedUser(existingUser, importedPrincipal, name, emailAddress, false);
        }

        [Fact]
        async void should_not_create_user_using_external_idp_enabled_when_register_not_allowed()
        {
            var userId = StringUtility.Random(5);
            MockExternalSignin(true, out var httpContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _app.GetService<ExternalSigninManager>()
                    .TransformToDiscussionUser(httpContext,
                        ComposeJwtClaims(userId, StringUtility.Random(), $"{StringUtility.Random(5)}@someplace.com", Guid.NewGuid().ToString("D")));
            });

            var userRepo = _app.GetService<IRepository<User>>();
            var importedUser = userRepo.All().FirstOrDefault(user => user.OpenId == userId && user.OpenIdProvider == "external");
            Assert.Null(importedUser);
        }

        [Fact]
        async void should_still_login_existing_users_using_external_idp_enabled_when_register_not_allowed()
        {
            var userId = StringUtility.Random(5);
            var name = StringUtility.Random();
            var emailAddress = $"{StringUtility.Random(5)}@someplace.com";

            MockExternalSignin(true, out var httpContext);
            
            var existingUser = _app.CreateUser(userId, displayName: name);
            existingUser.OpenId = userId;
            existingUser.EmailAddress = emailAddress;
            existingUser.OpenIdProvider = "external";
            var userRepo = _app.GetService<IRepository<User>>();
            userRepo.Update(existingUser);
            var transformedPrincipal = await _app.GetService<ExternalSigninManager>()
                .TransformToDiscussionUser(httpContext,
                    ComposeJwtClaims(userId, name, emailAddress, Guid.NewGuid().ToString("D")));
            
            Assert.NotNull(transformedPrincipal);
            VerifyTransformedUser(existingUser, transformedPrincipal, name, emailAddress, false);
        }

        private void MockExternalSignin(bool disableRegisteration, out HttpContext httpContext)
        {
            httpContext = new DefaultHttpContext();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.SetupGet(ctxAccessor => ctxAccessor.HttpContext).Returns(httpContext);

            _app.OverrideServices(services =>
            {
                services.AddSingleton(CreateMockExternalIdp());
                services.AddScoped<ExternalSigninManager>();
                services.AddSingleton(httpContextAccessorMock.Object);
                
                if (disableRegisteration)
                {
                    services.AddSingleton(new SiteSettings()
                    {
                        IsReadonly = false,
                        EnableNewUserRegistration = false
                    });
                }
            });
        }

        static IOptions<ExternalIdentityServiceOptions> CreateMockExternalIdp(bool enabled = true)
        {
            var externalIdpEnabledOptions = new Mock<IOptions<ExternalIdentityServiceOptions>>();
            externalIdpEnabledOptions.Setup(op => op.Value).Returns(new ExternalIdentityServiceOptions
            {
                IsEnabled = enabled,
                ProviderId = "external"
            });
            return externalIdpEnabledOptions.Object;
        }
        
        private static List<Claim> ComposeJwtClaims(string userId, string name, string emailAddress, string jwtTokenId)
        {
            var jwtClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, userId),
                new Claim(JwtClaimTypes.PreferredUserName, name),
                new Claim(JwtClaimTypes.Name, name),
                new Claim(JwtClaimTypes.Email, emailAddress),
                new Claim(JwtClaimTypes.EmailVerified, "true"),
                new Claim(JwtClaimTypes.PhoneNumber, StringUtility.RandomNumbers(8)),
                new Claim(JwtClaimTypes.PhoneNumberVerified, "true"),
                new Claim(JwtClaimTypes.JwtId, jwtTokenId)
            };
            return jwtClaims;
        }

        private static void VerifyTransformedUser(User transformedUser, ClaimsPrincipal importedPrincipal, string name, string emailAddress, bool emailConfirmed)
        {
            importedPrincipal.ShouldNotBeNull();
            Assert.NotNull(transformedUser);
            transformedUser.DisplayName.ShouldEqual(name);
            transformedUser.EmailAddress.ShouldEqual(emailAddress);
            transformedUser.ConfirmedEmail.ShouldEqual(emailConfirmed ? emailAddress : null);

            Assert.Equal(transformedUser.Id.ToString(),importedPrincipal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(name, importedPrincipal.FindFirst(c => c.Type == ClaimTypes.Name).Value);
        }

    }
}