using System.IO;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Discussion.Core;
using Discussion.Core.Data;
using Discussion.Core.FileSystem;
using Discussion.Core.Mvc;
using Discussion.Migrations.Supporting;
using Discussion.Web.Models;
using Discussion.Web.Resources;
using Discussion.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discussion.Web.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Discussion.Web.Services.EmailConfirmation;
using Discussion.Web.Services.Impl;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

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
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs));

            services.AddMvc(options =>
            {
                options.ModelBindingMessageProvider.UseTranslatedResources();
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add(new ApiResponseMvcFilter());
            });
            
            services.AddDataServices(_appConfiguration, _loggerFactory.CreateLogger<Startup>());
            services.AddIdentityServices();
            services.AddEmailSendingServices(_appConfiguration);
            services.AddSingleton<IContentTypeProvider>(new FileExtensionContentTypeProvider());
            services.AddSingleton<IFileSystem>(new LocalDiskFileSystem(Path.Combine(_hostingEnvironment.ContentRootPath, "uploaded")));
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddScoped<IUserAvatarService>(sp =>
                {
                    var actionAccessor = sp.GetService<IActionContextAccessor>();
                    var urlHelperFactory = sp.GetService<IUrlHelperFactory>();
                    return new UserAvatarService(urlHelperFactory.GetUrlHelper(actionAccessor.ActionContext));
                });
            
            
            var smsConfigSection = _appConfiguration.GetSection(nameof(AliyunSmsOptions));
            if (smsConfigSection != null && !string.IsNullOrEmpty(smsConfigSection[nameof(AliyunSmsOptions.AccountKeyId)]))
            {
                services.Configure<AliyunSmsOptions>(smsConfigSection);
                services.AddTransient<ISmsSender, AliyunSmsSender>();
            }
            else
            {
                services.AddTransient<ISmsSender, ConsoleSmsSender>();
            }
            
            var siteSettingsSection = _appConfiguration.GetSection(nameof(SiteSettings));
            if (siteSettingsSection != null)
            {
                services.Configure<SiteSettings>(siteSettingsSection);
            }
            services.AddSingleton(sp => sp.GetService<IOptions<SiteSettings>>().Value);
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
                if(bool.TryParse(_appConfiguration["HSTS"] ?? "False", out var useHsts) && useHsts)
                { 
                    app.UseHsts();
                }
            }
            
            app.UseHttpsRedirection();
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
