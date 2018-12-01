using System;
using System.Text;
using Discussion.Core.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Discussion.Admin.Supporting
{
    public static class JwtAuthenticationExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // jwt wire up Get options from app settings
            var jwtOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            var secret = jwtOptions["Secret"];
            var issuer = jwtOptions[nameof(JwtIssuerOptions.Issuer)];
            var audience = jwtOptions[nameof(JwtIssuerOptions.Audience)];
            var (signingKey, credentials) = GenerateSecurityKeys(secret);

            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = issuer;
                options.Audience = audience;
                options.SigningCredentials = credentials;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = issuer;
                configureOptions.SaveToken = true;

                configureOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        private static (SecurityKey, SigningCredentials) GenerateSecurityKeys(string secret)
        {
            var hasSetSecret = !string.IsNullOrEmpty(secret);
            SecurityKey signingKey;
            if (hasSetSecret)
            {
                signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
                return (signingKey, new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature));
            }

            signingKey = new RsaSecurityKey(EncryptProvider.GenerateParameters());
            return (signingKey, new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256));
        }
    }
}