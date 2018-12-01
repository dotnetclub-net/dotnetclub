using System;
using Discussion.Core.Models;
using Discussion.Web.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Web.Services.UserManagement.Identity
{
    public static class ServiceExtensions
    {
        private const string EmailConfirmationTokenProviderName = "EmailConfirmation";
        
        public static void AddIdentityServices(this IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddIdentity<User, Role>()
                .AddUserStore<RepositoryUserStore>()
                .AddRoleStore<NullRoleStore>()
                .AddClaimsPrincipalFactory<DiscussionUserClaimsPrincipalFactory>()
                .AddErrorDescriber<ResourceBasedIdentityErrorDescriber>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<EmailConfirmationTokenProvider<User>>(EmailConfirmationTokenProviderName);
            services.AddScoped<UserManager<User>, EmailAddressAwareUserManager<User>>();
            services.ConfigureApplicationCookie(options => options.LoginPath = "/signin");
            services.Configure<IdentityOptions>(options =>
            {
                // 我们在 SigninUserViewModel 中的 PasswordRules 类中进行验证
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = false; // 如果设置为 true，则不但会检查 Email 的唯一性，还会要求 Email 必填
                
                options.Tokens.EmailConfirmationTokenProvider = EmailConfirmationTokenProviderName;
            });
            
            services.Configure<EmailConfirmationTokenOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(7);
            });

        }
    }
}