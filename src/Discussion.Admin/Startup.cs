using System.Runtime.CompilerServices;
using Discussion.Admin.Services;
using Discussion.Admin.Services.Impl;
using Discussion.Admin.Supporting;
using Discussion.Core;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly:InternalsVisibleTo("Discussion.Admin.Tests")]
namespace Discussion.Admin
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJwtAuthentication(_appConfiguration);
            services.AddIdentityCore<AdminUser>();
            services.AddMvc(options => { options.Filters.Add(typeof(ApiResponseMvcFilter)); })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = ApiResponse.CamelCaseContractResolver;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            
            services.AddDataServices(_appConfiguration, _loggerFactory.CreateLogger<Startup>());

            services.AddScoped<IAdminUserService, AdminUserServiceImpl>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseApiResponse();
            app.UseAuthentication();
            app.UseMvc();
            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core, see https://go.microsoft.com/fwlink/?linkid=864501
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    //spa.UseAngularCliServer(npmScript: "start");
                    //   OR
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}