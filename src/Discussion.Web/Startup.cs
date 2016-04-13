using Discussion.Web.Data;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;

namespace Discussion.Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get;  }
        public IHostingEnvironment HostingEnvironment { get;  }
        public IApplicationEnvironment ApplicationEnvironment { get;  }

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            HostingEnvironment = env;
            ApplicationEnvironment = appEnv;
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
                    throw new ApplicationException("No configuration value set for key 'mongoConnectionString'");
                }

                // @jijiechen: detect at every time initate a new IRepositoryContext
                // may cause a performance issue
                if (!MongoDbUtils.DatabaseExists(mongoConnectionString))
                {
                    throw new ApplicationException("Could not find a database using specified connection string");
                }

                return new MongoRepositoryContext(mongoConnectionString);
            });
            services.AddScoped(typeof(Repository<,>), typeof(MongoRepository<,>));
            services.AddScoped(typeof(IDataRepository<>), typeof(BaseDataRepository<>));

            if (IsMono())
            {
                Console.WriteLine("Replaced default FileProvider with a wrapped synchronous one\n Since Mono has a bug on asynchronous filestream.\n See https://github.com/aspnet/Hosting/issues/604");
                UseSynchronousFileProvider(services, ApplicationEnvironment);
            }
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


        static void UseSynchronousFileProvider(IServiceCollection services, IApplicationEnvironment appEnv)
        {
            services.Configure<RazorViewEngineOptions>(opt =>
            {
                var physicalFileProvider = new PhysicalFileProvider(appEnv.ApplicationBasePath);
                opt.FileProvider = new WrappedSynchronousFileProvider(physicalFileProvider);
            });
        }

        static bool IsMono()
        {
            var runtime = PlatformServices.Default.Runtime;
            return runtime.RuntimeType.Equals("Mono", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class WrappedSynchronousFileProvider : IFileProvider
    {
        IFileProvider _original;
        public WrappedSynchronousFileProvider(IFileProvider original)
        {
            _original = original;
        }


        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _original.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var originalFileInfo = _original.GetFileInfo(subpath);
            var isPhysical = originalFileInfo.GetType().FullName.EndsWith("PhysicalFileInfo");
            if (!isPhysical)
            {
                return originalFileInfo;
            }

            return new WrappedSynchronousFileInfo(originalFileInfo);
        }

        public IChangeToken Watch(string filter)
        {
            return _original.Watch(filter);
        }


        public class WrappedSynchronousFileInfo : IFileInfo
        {
            IFileInfo _original;
            public WrappedSynchronousFileInfo(IFileInfo original)
            {
                _original = original;
            }


            public bool Exists
            {
                get
                {
                    return _original.Exists;
                }
            }

            public bool IsDirectory
            {
                get
                {
                    return _original.IsDirectory;
                }
            }

            public DateTimeOffset LastModified
            {
                get
                {
                    return _original.LastModified;
                }
            }

            public long Length
            {
                get
                {
                    return _original.Length;
                }
            }

            public string Name
            {
                get
                {
                    return _original.Name;
                }
            }

            public string PhysicalPath
            {
                get
                {
                    return _original.PhysicalPath;
                }
            }

            public Stream CreateReadStream()
            {
                // @jijiechen: replaced implemention from https://github.com/aspnet/FileSystem/blob/32822deef3fd59b848842a500a3e989182687318/src/Microsoft.Extensions.FileProviders.Physical/PhysicalFileInfo.cs#L30
                //return new FileStream(
                //    PhysicalPath,
                //    FileMode.Open,
                //    FileAccess.Read,
                //    FileShare.ReadWrite,
                //    1024 * 64,
                //    FileOptions.Asynchronous | FileOptions.SequentialScan);


                // Note: Buffer size must be greater than zero, even if the file size is zero.
                return new FileStream(
                   PhysicalPath,
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.ReadWrite,
                   1024 * 64,
                   FileOptions.SequentialScan);
            }
        }
    }

}
