using Discussion.Web.Data.InMemory;
using Discussion.Web.Data;
using Jusfr.Persistent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client;
using Raven.Client.Document;
using System;
using System.IO;
using System.Linq;

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

        public static void ConfigureHost(IWebHostBuilder hostBuilder, bool addCommandLineArguments = false)
        {
            var basePath = Directory.GetCurrentDirectory();
            var configBuilder = BuildHostingConfiguration(basePath, addCommandLineArguments ? Environment.GetCommandLineArgs() : null);
            var configuration = configBuilder.Build();

            hostBuilder.UseContentRoot(basePath)
                .UseIISIntegration()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseConfiguration(configuration);
        }

        // ConfigureServices is invoked before Configure
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddMvc();
            AddDataServicesTo(services, Configuration);
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
            var ravenConnectionString = _configuration["ravenConnectionString"];

            if (!string.IsNullOrWhiteSpace(ravenConnectionString)) {

                services.AddSingleton(new Lazy<IDocumentStore>(() =>
                {
                    var store = new DocumentStore();
                    store.ParseConnectionString(ravenConnectionString);
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
