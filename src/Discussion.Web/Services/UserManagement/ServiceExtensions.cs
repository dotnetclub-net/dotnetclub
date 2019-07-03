using System.IdentityModel.Tokens.Jwt;
using Discussion.Core.Communication.Email;
using Discussion.Core.Communication.Sms;
using Discussion.Core.Models.UserAvatar;
using Discussion.Web.Services.UserManagement.Avatar;
using Discussion.Web.Services.UserManagement.Avatar.UrlGenerators;
using Discussion.Web.Services.UserManagement.EmailConfirmation;
using Discussion.Web.Services.UserManagement.Identity;
using Discussion.Web.Services.UserManagement.PhoneNumberVerification;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Discussion.Web.Services.UserManagement
{
    public static class ServiceExtensions
    {
        public static void AddUserManagementServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
            
            services.AddIdentityServices();
            services.AddEmailServices(appConfiguration);
            services.AddSmsServices(appConfiguration);
            
            services.AddSingleton<IAvatarUrlService, DispatchAvatarUrlService>();
            services.AddScoped<IUserAvatarUrlGenerator<DefaultAvatar>, DefaultAvatarUrlGenerator>();
            services.AddScoped<IUserAvatarUrlGenerator<StorageFileAvatar>, StorageFileAvatarUrlGenerator>();
            services.AddScoped<IUserAvatarUrlGenerator<GravatarAvatar>, GravatarAvatarUrlGenerator>();
            
            services.AddScoped<IPhoneNumberVerificationService, DefaultPhoneNumberVerificationService>();
            services.AddSingleton<IConfirmationEmailBuilder, DefaultConfirmationEmailBuilder>();
            services.AddSingleton<IResetPasswordEmailBuilder, DefaultResetPasswordEmailBuilder>();
            
            services.AddScoped<IUserService, DefaultUserService>();
            
            var idpConfig = appConfiguration.GetSection(nameof(IdentityServerOptions));
            if (idpConfig != null)
            {
                services.Configure<IdentityServerOptions>(idpConfig);
            }

            var parsedConfiguration = idpConfig?.Get<IdentityServerOptions>();
            if (parsedConfiguration != null && parsedConfiguration.IsEnable)
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.Cookie.Name = "IdentityServerAdmin";
                            
                            // Issue: https://github.com/aspnet/Announcements/issues/318
                            options.Cookie.SameSite = SameSiteMode.None;
                        })
                    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                    {
                        options.Authority = parsedConfiguration.Authority;
                        options.RequireHttpsMetadata = parsedConfiguration.RequireHttpsMetadata;
                        options.ClientId = parsedConfiguration.ClientId;
                        options.ClientSecret = parsedConfiguration.ClientSecret;
                        options.SaveTokens = true;
                        options.ResponseType = "code id_token";
                        options.GetClaimsFromUserInfoEndpoint = true;
                        
                        options.Scope.Clear();
                        options.Scope.Add(OidcConstants.StandardScopes.OpenId);
                        options.Scope.Add(OidcConstants.StandardScopes.Profile);
                        options.Scope.Add(OidcConstants.StandardScopes.Email);

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            NameClaimType = JwtClaimTypes.Name,
                            RoleClaimType = JwtClaimTypes.Role
                        };
                    });
            }
        }
    }
}