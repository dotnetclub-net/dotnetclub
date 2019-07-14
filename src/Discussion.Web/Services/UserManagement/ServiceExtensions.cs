using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Communication.Email;
using Discussion.Core.Communication.Sms;
using Discussion.Core.Middleware;
using Discussion.Core.Models.UserAvatar;
using Discussion.Web.Services.UserManagement.Avatar;
using Discussion.Web.Services.UserManagement.Avatar.UrlGenerators;
using Discussion.Web.Services.UserManagement.EmailConfirmation;
using Discussion.Web.Services.UserManagement.Identity;
using Discussion.Web.Services.UserManagement.PhoneNumberVerification;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
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
            if (parsedConfiguration != null && parsedConfiguration.IsEnabled)
            {
                ConfigureExternalIdp(services, parsedConfiguration);
            }
        }

        private static void ConfigureExternalIdp(IServiceCollection services, IdentityServerOptions parsedConfiguration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.CallbackPath = "/oidc-callback";
                    options.RemoteSignOutPath = "/k_logout";
                    
                    options.Authority = parsedConfiguration.Authority;
                    options.RequireHttpsMetadata = parsedConfiguration.RequireHttpsMetadata;
                    options.ClientId = parsedConfiguration.ClientId;
                    options.ClientSecret = parsedConfiguration.ClientSecret;
                    options.ResponseType = "code id_token";
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Clear();
                    options.Scope.Add(OidcConstants.StandardScopes.OpenId);
                    options.Scope.Add(OidcConstants.StandardScopes.Profile);
                    options.Scope.Add(OidcConstants.StandardScopes.Email);
                    options.Scope.Add(OidcConstants.StandardScopes.Phone);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = parsedConfiguration.ClientId,
                        ValidIssuer =  parsedConfiguration.Authority
                    };
                    options.Events.OnAuthorizationCodeReceived = context =>
                    {
                        var jti = context.JwtSecurityToken.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.JwtId)?.Value;
                        context.TokenEndpointRequest.SetParameter("client_session_state", jti);   // session key id
//                        context.TokenEndpointRequest.SetParameter("client_session_host", "localhost:8080");  // web server instance name...   https://${application.session.host}.app.com/k_logout
                        return Task.CompletedTask;
                    };
                    options.Events.OnRemoteSignOut = async context =>
                    {
                        if (string.Equals(context.Request.Method, HttpMethod.Post.ToString(), StringComparison.OrdinalIgnoreCase) 
                            && context.Request.ContentLength != null && context.Request.ContentLength.Value > 0)
                        {
                            var body = context.Request.Body;
                            var content = await new StreamReader(body).ReadToEndAsync();
                            if (context.Options.SecurityTokenValidator.CanReadToken(content))
                            {
                                var idpServerConfiguration = await context.Options.ConfigurationManager.GetConfigurationAsync(CancellationToken.None);
                                SecurityToken validatedToken;
                                ClaimsPrincipal claimsPrincipal = context.Options.SecurityTokenValidator.ValidateToken(
                                    content,
                                    new TokenValidationParameters()
                                    {
                                        IssuerSigningKeys =  idpServerConfiguration.SigningKeys,
                                        ValidateAudience = false,
                                        ValidateIssuer = false,
                                        ValidateLifetime = false
                                    }, out validatedToken);

                                var sid = claimsPrincipal.FindFirst(c => c.Type == "adapterSessionIds")?.Value;
                                if (sid != null)
                                {
                                    RejectRevokedSessionMiddleware.RevokedTokens.Add(sid);
                                }
                            }
                            
                        }
                        context.HandleResponse();
                    };

                });
        }
    }
}