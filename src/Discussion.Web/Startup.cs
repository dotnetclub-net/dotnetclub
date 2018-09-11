using Discussion.Web.Data.InMemory;
using Discussion.Web.Data;
using Jusfr.Persistent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Discussion.Web.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Raven.Client.Documents;

namespace Discussion.Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get;  }
        public IHostingEnvironment HostingEnvironment { get;  }

        public Startup(IHostingEnvironment env)
        {
            HostingEnvironment = env;
            Configuration = BuildApplicationConfiguration(env.ContentRootPath, env.EnvironmentName).Build();
        }

        public static void Main(string[] args)
        {
            var hostBuilder = new WebHostBuilder();
            ConfigureHost(hostBuilder, addCommandLineArguments: true);

            var host = hostBuilder.Build();
            host.Run();
        }

        public static IWebHostBuilder ConfigureHost(IWebHostBuilder hostBuilder, bool addCommandLineArguments = false)
        {
            var basePath = Directory.GetCurrentDirectory();
            var configBuilder = BuildHostingConfiguration(basePath, addCommandLineArguments ? Environment.GetCommandLineArgs() : null);
            var configuration = configBuilder.Build();

            return hostBuilder.UseContentRoot(basePath)
                .UseIISIntegration()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                    loggingBuilder.AddConsole();
                });
        }

        // ConfigureServices is invoked before Configure
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddMvc();

            AddDataServicesTo(services, Configuration);

            services.AddSingleton(this.Configuration);
            services.AddAuthorization();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = new PathString("/signin");
                    o.AccessDeniedPath = new PathString("/access-denied");
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }
            
            app.UseAuthentication();
            app.Use((httpContext, next) =>
            {
                httpContext.AssignDiscussionPrincipal();
                return next();
            });
            app.UseStaticFiles();
            app.UseMvc();


            var ravenStore = app.ApplicationServices.GetService<Lazy<IDocumentStore>>();
            if (ravenStore != null)
            {
                var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
                lifetime.ApplicationStopping.Register(() =>
                {
                    if (ravenStore.IsValueCreated && !ravenStore.Value.WasDisposed)
                    {
                        ravenStore.Value.Dispose();
                    }
                });
            }
        }

        static IConfigurationBuilder BuildApplicationConfiguration(string basePath, string envName)
        {
            var builder = new ConfigurationBuilder()
                              .AddEnvironmentVariables(prefix: "OPENASPNETORG_")
                              .SetBasePath(basePath)
                              .AddJsonFile("appsettings.json", optional: true)
                              .AddJsonFile($"appsettings.{envName}.json", optional: true);

            return builder;
        }

        static IConfigurationBuilder BuildHostingConfiguration(string basePath, string[] commandlineArgs = null)
        {
            var builder = new ConfigurationBuilder()
                              .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                              .SetBasePath(basePath)
                              .AddJsonFile("hosting.json", optional: true);

            if (commandlineArgs != null)
            {
                var args = Environment.GetCommandLineArgs();
                var firstArgs = args.FirstOrDefault(arg => arg.StartsWith("--"));
                var argsIndex = Array.IndexOf(args, firstArgs);

                if (argsIndex > -1)
                {
                    var usefulArgs = args.Skip(argsIndex).ToArray();
                    builder.AddCommandLine(usefulArgs);
                }
            }

            return builder;
        }

        static void AddDataServicesTo(IServiceCollection services, IConfiguration _configuration)
        {
            var ravenServerUrl = _configuration["ravenServerUrl"];
            var ravenDatabase = _configuration["ravenDbName"];

            if (!string.IsNullOrWhiteSpace(ravenServerUrl)) {

                services.AddSingleton(new Lazy<IDocumentStore>(() =>
                {
                    var store = new DocumentStore()
                    {
                        Urls = new[] {ravenServerUrl},
                        Database = ravenDatabase
                    };
                    store.Initialize();

                    return store;
                }));

                services.AddScoped(typeof(IRepositoryContext), (serviceProvider) => {
                    return new RavenRepositoryContext(() =>
                    {
                        return serviceProvider.GetService<Lazy<IDocumentStore>>().Value;
                    });
                });
                services.AddScoped(typeof(Repository<>), typeof(RavenDataRepository<>));
                services.AddScoped(typeof(IRepository<>), typeof(RavenDataRepository<>));
                
                return;
            }

            var dataContext = new InMemoryResponsitoryContext();
            services.AddSingleton(typeof(IRepositoryContext), (serviceProvider) => dataContext);
            services.AddScoped(typeof(Repository<>), typeof(InMemoryDataRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(InMemoryDataRepository<>));
        }
    }
}
