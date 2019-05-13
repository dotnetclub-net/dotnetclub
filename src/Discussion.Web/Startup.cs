using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Discussion.Core;
using Discussion.Core.Cryptography;
using Discussion.Core.Data;
using Discussion.Core.FileSystem;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Time;
using Discussion.Migrations.Supporting;
using Discussion.Web.Resources;
using Discussion.Web.Services.ChatHistoryImporting;
using Discussion.Web.Services.TopicManagement;
using Discussion.Web.Services.UserManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Discussion.Core.ETag;
using Discussion.Core.Logging;
using Discussion.Core.Middleware;

namespace Discussion.Web
{
    public class Startup
    {
        private readonly IConfiguration _appConfiguration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<Startup> _startupLogger;

        public Startup(IHostingEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _hostingEnvironment = env;
            _appConfiguration = config;
            _startupLogger = loggerFactory.CreateLogger<Startup>();
        }

        private static void Main()
        {
            var host = WebHostConfiguration
                .Configure(new WebHostBuilder(), addCommandLineArguments: true)
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        // ConfigureServices is invoked before Configure
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs));

            services.ConfigureDataProtection(_appConfiguration);
            services.AddMvc(options =>
            {
                options.ModelBindingMessageProvider.UseTranslatedResources();
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add(new ApiResponseMvcFilter());
            });

           services.AddDataServices(_appConfiguration, _startupLogger);

           services.AddSingleton<IClock, SystemClock>();
           services.AddSingleton<HttpMessageInvoker>(new HttpClient());

           services.AddSingleton<IContentTypeProvider>(new FileExtensionContentTypeProvider());
           services.AddSingleton<IFileSystem>(new LocalDiskFileSystem(Path.Combine(_hostingEnvironment.ContentRootPath, "uploaded")));
           services.AddScoped<ITagBuilder, ETagBuilder>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddScoped(sp =>
                {
                    var actionAccessor = sp.GetService<IActionContextAccessor>();
                    var urlHelperFactory = sp.GetService<IUrlHelperFactory>();
                    return urlHelperFactory.GetUrlHelper(actionAccessor.ActionContext);
                });

            services.AddUserManagementServices(_appConfiguration);
            services.AddChatyImportingServices(_appConfiguration);

            services.AddScoped<ITopicService, DefaultTopicService>();

            // todo: cache site settings!
            services.AddScoped(sp =>
            {
                var stored = sp.GetService<IRepository<SiteSettings>>().All().FirstOrDefault();
                return stored ??
                       new SiteSettings
                       {
                           EnableNewReplyCreation = true,
                           EnableNewTopicCreation = true,
                           EnableNewUserRegistration = true
                       };
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseTracingId();
            SetupGlobalExceptionHandling(app);
            SetupHttpsSupport(app);

            app.UseMiddleware<SiteReadonlyMiddleware>();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc();

            InitDatabaseIfNeeded(app);
        }

        private void SetupHttpsSupport(IApplicationBuilder app)
        {
            if (!_hostingEnvironment.IsDevelopment()
                && bool.TryParse(_appConfiguration["HSTS"] ?? "False", out var useHsts)
                && useHsts)
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
        }

        private void SetupGlobalExceptionHandling(IApplicationBuilder app)
        {
            if (_hostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }
        }

        private void InitDatabaseIfNeeded(IApplicationBuilder app)
        {
            app.EnsureDatabase(connStr =>
            {
                _startupLogger.LogCritical("正在创建新的数据库结构...");

                var loggingConfig = _appConfiguration.GetSection(WebHostConfiguration.ConfigKeyLogging);
                DatabaseMigrator.Migrate(connStr, migrationLogging =>
                    FileLoggingExtensions.AddSeriFileLogger(migrationLogging, loggingConfig /* enable full logging for migrations */));

                _startupLogger.LogCritical("数据库结构创建完成");
            }, _startupLogger);
        }
    }
}
