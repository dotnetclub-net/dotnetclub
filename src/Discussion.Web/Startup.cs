using Discussion.Web.Repositories;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;


namespace Discussion.Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; private set; }

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            Configuration = BuildConfiguration(env, appEnv);
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddMvc();


            services.AddScoped(typeof(IRepositoryContext), (serviceProvider) =>
            {
                var mongoConnectionString = Configuration["mongoConnectionString"];
                if (string.IsNullOrWhiteSpace(mongoConnectionString))
                {
                    throw new System.ApplicationException("No configuration value set for key 'mongoConnectionString'");
                }

                return new MongoRepositoryContext(mongoConnectionString);
            });
            services.AddScoped(typeof(Repository<,>), typeof(MongoRepository<,>));
            services.AddScoped(typeof(IDataRepository<>), typeof(BaseDataRepository<>));
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

            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();
            app.UseStaticFiles();
            app.UseMvc();

        }


        private static IConfigurationRoot BuildConfiguration(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(appEnv.ApplicationBasePath)
              .AddJsonFile("appsettings.json", optional: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            return builder.Build();
        }

    }
}
