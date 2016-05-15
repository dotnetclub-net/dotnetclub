using Discussion.Web.Data;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Startup;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;
using Xunit;
using static Discussion.Web.Tests.TestEnv;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;

namespace Discussion.Web.Tests
{
    public sealed class Application: IApplicationContext, IDisposable
    {
        TestApplicationContext _applicationContext;
        Database _database;
        public TestApplicationContext Context
        {
            get
            {
                if(_applicationContext != null)
                {
                    return _applicationContext;
                }

                Action<IServiceCollection> setupServices = (services) =>
                {
                    var dataContext = new InMemoryResponsitoryContext();
                    services.AddScoped(typeof(IRepositoryContext), (serviceProvider) => dataContext);
                    services.AddScoped(typeof(Repository<,>), typeof(InMemoryDataRepository<,>));
                };

                _applicationContext = BuildApplication( (c)=> { }, setupServices, (a, h, l) => { } );
                return _applicationContext;
            }
        }


        #region Proxy Context Properties

        public StubLoggerFactory LoggerFactory { get { return this.Context.LoggerFactory; } }
        public IApplicationEnvironment ApplicationEnvironment { get { return this.Context.ApplicationEnvironment; }  }
        public IHostingEnvironment HostingEnvironment { get { return this.Context.HostingEnvironment; }  }

        public RequestDelegate RequestHandler { get { return this.Context.RequestHandler; }  }
        public IServiceProvider ApplicationServices { get { return this.Context.ApplicationServices; }  }

        public IConfigurationRoot Configuration { get { return this.Context.Configuration; }  }
        public TestServer Server { get { return this.Context.Server; }  }

        #endregion


        public static TestApplicationContext BuildApplication(Action<IConfigurationBuilder> customConfiguration, Action<IServiceCollection> serviceConfiguration, Action<IApplicationBuilder, IHostingEnvironment, ILoggerFactory> serviceCustomConfiguration, string environmentName = "Production")
        {
            StartupMethods startup = null;
            IApplicationBuilder bootingupApp = null;
            var bootstrapDiagnosticMessages = new List<string>();


            var testApp = new TestApplicationContext
            {
                LoggerFactory = new StubLoggerFactory(),
                ApplicationEnvironment = CreateApplicationEnvironment(),
                HostingEnvironment = new HostingEnvironment { EnvironmentName = environmentName }
            };
            testApp.Configuration = BuildConfiguration(testApp.HostingEnvironment, testApp.ApplicationEnvironment, customConfiguration);


            Func<IServiceCollection, IServiceProvider> configureServices = services =>
            {
                services.AddInstance<ILoggerFactory>(testApp.LoggerFactory);
                services.AddInstance<IApplicationEnvironment>(testApp.ApplicationEnvironment);

                var loader = new StartupLoader(services.BuildServiceProvider(), testApp.HostingEnvironment);
                startup = loader.LoadMethods(typeof(Startup), bootstrapDiagnosticMessages);
                startup.ConfigureServicesDelegate(services);
                serviceConfiguration(services);

                testApp.ApplicationServices = services.BuildServiceProvider();
                return testApp.ApplicationServices;
            };
            Action<IApplicationBuilder> configure = app =>
            {
                bootingupApp = app;
                startup.ConfigureDelegate(app);
                serviceCustomConfiguration(app, testApp.HostingEnvironment, testApp.ApplicationServices.GetService<ILoggerFactory>());
            };


            var webHostBuilder = TestServer
                                .CreateBuilder(testApp.Configuration)
                                .UseEnvironment(environmentName)
                                .UseStartup(configure, configureServices);

            testApp.Server = new TestServer(webHostBuilder);
            testApp.RequestHandler = bootingupApp.Build();

            return testApp;
        }

        static IConfigurationRoot BuildConfiguration(IHostingEnvironment env, IApplicationEnvironment appEnv, Action<IConfigurationBuilder> customConfiguration)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(appEnv.ApplicationBasePath)
              .AddJsonFile("appsettings.json", optional: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            customConfiguration(builder);

            return builder.Build();
        }

        internal static IApplicationEnvironment CreateApplicationEnvironment()
        {
            var env = new TestApplicationEnvironment();
            env.ApplicationBasePath = WebProjectPath();
            env.ApplicationName = "Discussion.Web";

            return env;
        }

        #region Disposing

        ~Application()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }


            if (_database != null)
            {
                (_database as IDisposable).Dispose();
                _database = null;
            }

            if(_applicationContext != null)
            {
                (_applicationContext as IDisposable).Dispose();
                _applicationContext = null;
            }
        }

        #endregion
    }

    // Use shared context to maintain database fixture
    // see https://xunit.github.io/docs/shared-context.html#collection-fixture
    [CollectionDefinition("AppSpecs")]
    public class ApplicationCollection : ICollectionFixture<Application>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public interface IApplicationContext: IDisposable
    {
         StubLoggerFactory LoggerFactory { get;  }
         IApplicationEnvironment ApplicationEnvironment { get;  }
         IHostingEnvironment HostingEnvironment { get;  }

         RequestDelegate RequestHandler { get;  }
         IServiceProvider ApplicationServices { get;  }

         IConfigurationRoot Configuration { get;  }
         TestServer Server { get;  }
    }

    public class TestApplicationContext: IApplicationContext
    {
        public StubLoggerFactory LoggerFactory { get; set; }
        public IApplicationEnvironment ApplicationEnvironment { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }        

        public RequestDelegate RequestHandler { get; set; }
        public IServiceProvider ApplicationServices { get; set; }

        public IConfigurationRoot Configuration { get; set; }
        public TestServer Server { get; set; }




        #region Disposing

        ~TestApplicationContext()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            RequestHandler = null;
            ApplicationServices = null;

            if (LoggerFactory != null)
            {
                LoggerFactory.LogItems.Clear();
                LoggerFactory = null;
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        #endregion
    }

    public class StubLoggerFactory : ILoggerFactory
    {
        public LogLevel MinimumLevel
        {
            get
            {
                return LogLevel.Debug;
            }

            set
            {

            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger { Factory = this };
        }

        public ConcurrentStack<LogItem> LogItems { get; private set; } = new ConcurrentStack<LogItem>();

        public void Dispose()
        {
            LogItems.Clear();
            LogItems = null;
        }

        public class Logger : ILogger
        {
            public StubLoggerFactory Factory { get; set; }


            public IDisposable BeginScopeImpl(object state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                var log = new LogItem
                {
                    Level = logLevel,
                    EventId = eventId,
                    State = state,
                    Exception = exception,
                    Message = formatter.Invoke(state, exception)
                };
                Factory.LogItems.Push(log);
            }
        }

        public class LogItem
        {
            public LogLevel Level { get; set; }
            public Exception Exception { get; set; }
            public int EventId { get; set; }
            public object State { get; set; }
            public string Message { get; set; }
        }
    }

    class TestApplicationEnvironment : IApplicationEnvironment
    {
        public string ApplicationBasePath { get; set; }

        public string ApplicationName { get; set; }

        public string ApplicationVersion => PlatformServices.Default.Application.ApplicationVersion;

        public string Configuration => PlatformServices.Default.Application.Configuration;

        public FrameworkName RuntimeFramework => PlatformServices.Default.Application.RuntimeFramework;

        public object GetData(string name)
        {
            return PlatformServices.Default.Application.GetData(name);
        }

        public void SetData(string name, object value)
        {
            PlatformServices.Default.Application.SetData(name, value);
        }
    }

    public sealed class Database : IDisposable
    {
        string _connectionString;
        private Database(string databaseServer)
        {
            _connectionString = CreateConnectionStringForTesting(databaseServer);
            Context = new MongoRepositoryContext(_connectionString);
        }

        public MongoRepositoryContext Context { get; private set; }
        public static Database Create(string databaseServer)
        {
            return new Database(databaseServer);
        }
        public static Database CreateDefault()
        {
            return Create("192.168.1.178:27017");
        }


        #region Disposing

        ~Database()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            try
            {
                Context.Dispose();
                DropDatabase(_connectionString);
            }
            catch { }
        }

        #endregion

        #region helpers for setting up database

        static string CreateConnectionStringForTesting(string host)
        {
            string connectionString;

            do
            {
                var databaseName = string.Concat("ut_" + DateTime.UtcNow.ToString("yyyyMMddHHMMss") + Guid.NewGuid().ToString("N").Substring(0, 4));
                connectionString = $"mongodb://{host}/{databaseName}";
            }
            while (MongoDbUtils.DatabaseExists(connectionString));
            return connectionString;
        }

        static void DropDatabase(string connectionString)
        {
            var mongoUri = new MongoUrl(connectionString);
            //new MongoClient(mongoUri).DropDatabase(mongoUri.DatabaseName);
            new MongoClient(mongoUri).DropDatabaseAsync(mongoUri.DatabaseName).Wait();
        }

        #endregion

    }

    // services.AddScoped(typeof(Repository<,>), typeof(MongoRepository<,>));
    public class InMemoryDataRepository<TEntry, TKey> : Repository<TEntry, TKey> where TEntry : class, IAggregate<TKey>
    {
        public InMemoryDataRepository(IRepositoryContext dataContext) : base(dataContext)
        {

        }

        private InMemoryResponsitoryContext StorageContext
        {
            get
            {
                return Context as InMemoryResponsitoryContext;
            }
        }

        public override IQueryable<TEntry> All
        {
            get
            {
                var repo = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
                return repo.Values.AsQueryable();
            }
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates)
        {
            IQueryable<TEntry> query = All;
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
            return query.Select(r => r.Id).Any();
        }

        public override void Create(TEntry entry)
        {
            var repo = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            entry.Id = GenerateNewId(repo.Count);
            repo.TryAdd(entry.Id, entry);
        }

        public override void Delete(IEnumerable<TEntry> entries)
        {
            var repo = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            var idList = entries.Where(e => e != null && repo.ContainsKey(e.Id)).Select(e => e.Id).ToArray();

            foreach (var id in idList)
            {
                TEntry val;
                repo.TryRemove(id, out val);
            }
        }

        public override void Delete(TEntry entry)
        {
            Delete(new[] { entry });
        }

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query)
        {
            return query(All);
        }

        public override IEnumerable<TEntry> Retrive(params TKey[] keys)
        {
            if(keys == null)
            {
                yield break;
            }

            var storage = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            foreach(var key in keys)
            {
                TEntry val;
                storage.TryGetValue(key, out val);
                yield return val;
            }
        }

        public override TEntry Retrive(TKey id)
        {
            return Retrive(new[] { id }).FirstOrDefault();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys)
        {
            var storage = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            //var memberSelector = selector.Compile();
            //return All
            //    .Select(entry => new
            //    {
            //        Entry = entry,
            //        Member = memberSelector(entry)
            //    })
            //    .Where(item => keys.Contains(item.Member))
            //    .Select(item => item.Entry)
            //    .ToList();

            var parameters = selector.Parameters;
            var memberValue = Expression.Invoke(selector, parameters);

            Expression<Func<TMember, bool>> contains = member => keys.Contains(member);
            var containsMember = Expression.Invoke(contains, memberValue);
            var valueSelector = Expression.Lambda(containsMember, parameters) as Expression<Func<TEntry, bool>>;

            return All.Where(valueSelector).ToList();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(string field, params TMember[] keys)
        {
            var entryParameter = Expression.Parameter(typeof(TEntry), "entry");
            var memberExpr = Expression.PropertyOrField(entryParameter, field);
            var selector = Expression.Lambda(memberExpr, entryParameter) as Expression<Func<TEntry, TMember>>;

            return Retrive(selector, keys);
        }

        public override void Save(IEnumerable<TEntry> entries)
        {
            var storage = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            var count = storage.Count;
            foreach (var item in entries)
            {
                if (item.Id.Equals(default(TKey)))
                {
                    item.Id = GenerateNewId(++count);
                }

                storage.AddOrUpdate(item.Id, item, (key, existingValue) => item);
            }
        }

        public override void Save(TEntry entry)
        {
            Save(new[] { entry });
        }

        public override void Update(IEnumerable<TEntry> entries)
        {
            var storage = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            foreach (var item in entries)
            {
                var existing = Retrive(item.Id);
                storage.TryUpdate(item.Id, item, existing);
            }
        }

        public override void Update(TEntry entry)
        {
            Update(new[] { entry });
        }

        TKey GenerateNewId(int count)
        {
            return (TKey)Convert.ChangeType(count + 1, typeof(TKey));
        }
    }

    public class InMemoryResponsitoryContext : DisposableObject, IRepositoryContext
    {
        private ConcurrentDictionary<string, object> _storage = new ConcurrentDictionary<string, object>();

        public bool DistributedTransactionSupported { get; } = false;

        public ConcurrentDictionary<TKey, TEntry> GetRepositoryForEntity<TKey, TEntry>()
        {
            var type = typeof(TEntry).FullName;
            var entryStorage = _storage.GetOrAdd(type, typeName =>
            {
                return new ConcurrentDictionary<TKey, TEntry>();
            });

            return entryStorage as ConcurrentDictionary<TKey, TEntry>;
        }

        public Guid ID { get; } = Guid.NewGuid();

        public void Begin()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        protected override void DisposeManaged()
        {
            foreach (var key in _storage.Keys)
            {
                var dic = _storage[key] as IDictionary;
                if (dic != null)
                {
                    dic.Clear();
                }
            }

            _storage.Clear();
        }
    }

    public static class TestApplicationContextExtensions
    {
        public static T CreateController<T>(this IApplicationContext app) where T : Controller
        {
            var services = app.ApplicationServices;

            var actionContext = new ActionContext(
                new DefaultHttpContext
                {
                    RequestServices = services
                },
                new RouteData(),
                new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(T).GetTypeInfo()
                });

            var actionBindingContext = GetActionBindingContext(services.GetService<IOptions<MvcOptions>>().Value, actionContext);
            services.GetRequiredService<IActionBindingContextAccessor>().ActionBindingContext = actionBindingContext;

            var controllerFactory = services.GetService<IControllerFactory>();
            return controllerFactory.CreateController(actionContext) as T;
        }

        private static ActionBindingContext GetActionBindingContext(MvcOptions options, ActionContext actionContext)
        {
            var valueProviderFactoryContext = new ValueProviderFactoryContext(actionContext.HttpContext, actionContext.RouteData.Values);
            var valueProvider = CompositeValueProvider.CreateAsync(options.ValueProviderFactories, valueProviderFactoryContext).Result;

            return new ActionBindingContext()
            {
                InputFormatters = options.InputFormatters,
                OutputFormatters = options.OutputFormatters,
                ValidatorProvider = new CompositeModelValidatorProvider(options.ModelValidatorProviders),
                ModelBinder = new CompositeModelBinder(options.ModelBinders),
                ValueProvider = valueProvider
            };
        }

        public static T GetService<T>(this IApplicationContext app) where T : class
        {
            return app.ApplicationServices.GetService<T>();
        }
    }
}
