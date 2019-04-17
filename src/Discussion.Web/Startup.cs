using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Discussion.Core;
using Discussion.Core.Communication.Email;
using Discussion.Core.Communication.Sms;
using Discussion.Core.Cryptography;
using Discussion.Core.Data;
using Discussion.Core.FileSystem;
using Discussion.Core.Models;
using Discussion.Core.Models.UserAvatar;
using Discussion.Core.Mvc;
using Discussion.Core.Time;
using Discussion.Migrations.Supporting;
using Discussion.Web.Resources;
using Discussion.Web.Services;
using Discussion.Web.Services.ChatHistoryImporting;
using Discussion.Web.Services.TopicManagement;
using Discussion.Web.Services.UserManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Discussion.Web.Services.UserManagement.Avatar;
using Discussion.Web.Services.UserManagement.Avatar.UrlGenerators;
using Discussion.Web.Services.UserManagement.EmailConfirmation;
using Discussion.Web.Services.UserManagement.Identity;
using Discussion.Web.Services.UserManagement.PhoneNumberVerification;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Discussion.Core.ETag;

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

            services.ConfigureDataProtection(_appConfiguration);
            services.AddMvc(options =>
            {
                options.ModelBindingMessageProvider.UseTranslatedResources();
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add(new ApiResponseMvcFilter());
            });

            services.AddSingleton<IClock, SystemClock>();
            services.AddDataServices(_appConfiguration, _loggerFactory.CreateLogger<Startup>());
            services.AddIdentityServices();
            services.AddEmailServices(_appConfiguration);
            services.AddSmsServices(_appConfiguration);

            services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
            services.AddSingleton<IContentTypeProvider>(new FileExtensionContentTypeProvider());
            services.AddSingleton<IFileSystem>(new LocalDiskFileSystem(Path.Combine(_hostingEnvironment.ContentRootPath, "uploaded")));
            services.AddSingleton<HttpMessageInvoker>(new HttpClient());

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddScoped(sp =>
                {
                    var actionAccessor = sp.GetService<IActionContextAccessor>();
                    var urlHelperFactory = sp.GetService<IUrlHelperFactory>();
                    return urlHelperFactory.GetUrlHelper(actionAccessor.ActionContext);
                });

            services.AddSingleton<IAvatarUrlService, DispatchAvatarUrlService>();
            services.AddScoped<IUserAvatarUrlGenerator<DefaultAvatar>, DefaultAvatarUrlGenerator>();
            services.AddScoped<IUserAvatarUrlGenerator<StorageFileAvatar>, StorageFileAvatarUrlGenerator>();
            services.AddScoped<IUserAvatarUrlGenerator<GravatarAvatar>, GravatarAvatarUrlGenerator>();
            services.AddScoped<IPhoneNumberVerificationService, DefaultPhoneNumberVerificationService>();
            services.AddSingleton<IConfirmationEmailBuilder, DefaultConfirmationEmailBuilder>();
            services.AddSingleton<IResetPasswordEmailBuilder, DefaultResetPasswordEmailBuilder>();
            services.AddScoped<IUserService, DefaultUserService>();
            services.AddScoped<ITagBuilder, ETagBuilder>();

            services.AddScoped<IChatHistoryImporter, DefaultChatHistoryImporter>();
            services.AddSingleton<INameGenerator, DefaultNameGenerator>();
            var chatyConfig = _appConfiguration.GetSection(nameof(ChatyOptions));
            if (chatyConfig != null && !string.IsNullOrEmpty(chatyConfig[nameof(ChatyOptions.ServiceBaseUrl)]))
            {
                services.Configure<ChatyOptions>(chatyConfig);
            }

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

            app.Use(async (ctx, next) =>
            {
                var readonlyDataSettings = ctx.RequestServices.GetService<IReadonlyDataSettings>() as ReadonlyDataSettings;
                Debug.Assert(readonlyDataSettings != null, nameof(readonlyDataSettings) + " != null");

                var siteSettings = ctx.RequestServices.GetService<SiteSettings>();
                readonlyDataSettings.IsReadonly = siteSettings.IsReadonly;
                await next();
            });
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
