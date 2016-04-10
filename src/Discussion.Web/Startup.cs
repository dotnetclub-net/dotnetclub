using Discussion.Web.Repositories;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using System;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNet.FileProviders;
using Microsoft.Extensions.Primitives;
using System.IO;

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

            if (IsMono())
            {
                Console.WriteLine("Replaced mvc default ICompilerCacheProvider by SyncFileProviderCompilerCacheProvider. \n Since Mono has a bug on asynchronous filestream.\n See https://github.com/aspnet/Hosting/issues/604");
                services.TryAddSingleton<ICompilerCacheProvider, SyncFileProviderCompilerCacheProvider>();
            }
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

        static bool IsMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

    }

    //class CoreReplacement : DefaultRazorViewEngineFileProviderAccessor
    //{

    //}


    public class SyncFileProviderCompilerCacheProvider : ICompilerCacheProvider
    {
        public SyncFileProviderCompilerCacheProvider(IOptions<RazorViewEngineOptions> mvcViewOptions)
        {
            var fileProvider = new WrappedFileProvider(mvcViewOptions.Value.FileProvider);
            Cache = new CompilerCache(fileProvider);
        }

        public ICompilerCache Cache { get; }


        public class WrappedFileProvider : IFileProvider
        {
            IFileProvider _original;
            public WrappedFileProvider(IFileProvider original)
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



}
