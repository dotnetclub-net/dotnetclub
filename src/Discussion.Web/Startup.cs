using Discussion.Web.Data;
using Discussion.Web.Data.InMemory;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
            var basePath = Environment.CurrentDirectory;
            var configBuilder = BuildHostingConfiguration(basePath, hostBuilder.GetSetting(WebHostDefaults.EnvironmentKey), addCommandLineArguments ? Environment.GetCommandLineArgs() : null);
            var configuration = configBuilder.Build();

            hostBuilder.UseContentRoot(basePath)
                .UseIISIntegration()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseConfiguration(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            // Configure runtime to enable specified characters to be rendered as is
            // See https://github.com/aspnet/HttpAbstractions/issues/315
            // services.AddWebEncoders(option =>
            // {
            //     var enabledChars = new[]
            //     {
            //         UnicodeRanges.BasicLatin,
            //         UnicodeRanges.Latin1Supplement,
            //         // UnicodeRanges.CJKUnifiedIdeographs,
            //         UnicodeRanges.HalfwidthandFullwidthForms,
            //         UnicodeRanges.LatinExtendedAdditional,
            //         UnicodeRanges.LatinExtendedA,
            //         UnicodeRanges.LatinExtendedB,
            //         UnicodeRanges.LatinExtendedC,
            //         UnicodeRanges.LatinExtendedD,
            //         UnicodeRanges.LatinExtendedE
            //     };

            //     option.CodePointFilter = new CodePointFilter(enabledChars);
            // });
            
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
                app.UseDeveloperExceptionPage();
             //   app.UseExceptionHandler("/error");
            }

            // Add the platform handler to the request pipeline.
            // app.UseIISPlatformHandler();
            app.UseStaticFiles();
            app.UseMvc();
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

        static IConfigurationBuilder BuildHostingConfiguration(string basePath, string defaultEnvName, string[] commandlineArgs = null)
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
            var mongoConnectionString = _configuration["mongoConnectionString"];
            var hasMongoCongured = !string.IsNullOrWhiteSpace(mongoConnectionString);


            if (hasMongoCongured)
            {
                services.AddScoped(typeof(IRepositoryContext), (serviceProvider) =>
                {
                // @jijiechen: detect at every time initate a new IRepositoryContext
                // may cause a performance issue
                if (!MongoDbUtils.DatabaseExists(mongoConnectionString))
                    {
                        throw new ApplicationException("Could not find a database using specified connection string");
                    }

                    return new MongoRepositoryContext(mongoConnectionString);
                });
                services.AddScoped(typeof(Repository<,>), typeof(MongoRepository<,>));
            }
            else
            {
                var dataContext = new InMemoryResponsitoryContext();
                services.AddScoped(typeof(IRepositoryContext), (serviceProvider) => dataContext);
                services.AddScoped(typeof(Repository<,>), typeof(InMemoryDataRepository<,>));
            }

            services.AddScoped(typeof(IDataRepository<>), typeof(BaseDataRepository<>));
        }
    }

}
