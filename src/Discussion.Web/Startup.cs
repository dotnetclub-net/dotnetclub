using System.Text.Encodings.Web;
using System.Text.Unicode;
using Discussion.Core;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Migrations.Supporting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discussion.Web.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Discussion.Web
{
    public class Startup
    {
        private readonly IConfiguration _appConfiguration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IHostingEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _hostingEnvironment = env;
            _appConfiguration = config;
            _loggerFactory = loggerFactory;
        }

        private static void Main()
        {
            var host = Configuration
                .ConfigureHost(new WebHostBuilder(), addCommandLineArguments: true)
                .UseStartup<Startup>()
                .Build();
            
            host.Run();
        }

        // ConfigureServices is invoked before Configure
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddIdentity<User, Role>()
                    .AddUserStore<RepositoryUserStore>()
                    .AddRoleStore<NullRoleStore>()
                    .AddClaimsPrincipalFactory<DiscussionUserClaimsPrincipalFactory>()
                    .AddDefaultTokenProviders();

            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs));
            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.AddDataServices(_appConfiguration, _loggerFactory.CreateLogger<Startup>());

            services.AddAuthorization();
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
                
//                options.User.RequireUniqueEmail = true;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_hostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }
            
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc();

            var logger = _loggerFactory.CreateLogger<Startup>();
            app.EnsureDatabase(connStr =>
            {
                logger.LogCritical("正在创建新的数据库结构...");

                var loggingConfig = _appConfiguration.GetSection(Configuration.ConfigKeyLogging);
                SqliteMigrator.Migrate(connStr, migrationLogging => Configuration.ConfigureFileLogging(migrationLogging, loggingConfig, true /* enable full logging for migrations */));

                logger.LogCritical("数据库结构创建完成");
            }, logger);
        }
        
    }
}
