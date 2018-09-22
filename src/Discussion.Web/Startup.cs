using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discussion.Web.ApplicationSupport;
using Discussion.Web.Models;
using Discussion.Web.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Discussion.Web
{
    public class Startup
    {
        public IConfiguration ApplicationConfiguration { get;  }
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IHostingEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _hostingEnvironment = env;
            _loggerFactory = loggerFactory;
            ApplicationConfiguration = config;
        }

        public static void Main(string[] args)
        {
            var host = Configurer.ConfigureHost(new WebHostBuilder(), addCommandLineArguments: true).Build();
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
            services.AddMvc();

            services.AddDataServices(ApplicationConfiguration, _loggerFactory);

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
            app.RegisterDatabaseInitializing();
        }
    }
}
