using System.IO;
using Discussion.Web.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.ApplicationSupport
{
    internal static class DataInfrastructure
    {
        const string ConfigKey_ConnectionString = "sqliteConnectionString";
        
        internal static void AddDataServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            var connectionString = NormalizeSqliteConnectionString(appConfiguration[ConfigKey_ConnectionString], out var useInMemory);
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString);   
            });
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        }

        internal static void RegisterDatabaseInitializing(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;
            var appConfiguration = services.GetService<IConfiguration>();
            var connectionString = NormalizeSqliteConnectionString(appConfiguration[ConfigKey_ConnectionString], out var useInMemory);

            var logger = services.GetService<ILogger<Startup>>();
            if (useInMemory)
            {
                logger.LogCritical("未配置数据库连接字符串，将使用内存数据库（进程停止后数据即丢失）。");
            }

            var loggingConfigurtion = appConfiguration.GetSection(Configurer.ConfigKey_Logging);
            services.GetService<IApplicationLifetime>()
                .ApplicationStarted
                .Register(() => InitializeDatabase(useInMemory, connectionString, logger, loggingConfigurtion));
        }

        static void InitializeDatabase(bool useInMemory, string connectionString, 
            ILogger<Startup> logger, IConfiguration loggingConfiguration)
        {
            var sqliteConnection = new SqliteConnection(connectionString);
            var createDatabase = useInMemory || !File.Exists(sqliteConnection.DataSource);
            if (createDatabase)
            {
                logger.LogCritical("正在创建新的数据库结构...");
                
                DatabaseMigrator.Migrate(connectionString, loggingConfiguration);
                
                logger.LogCritical("数据库结构创建完成");
            }
        }

        static string NormalizeSqliteConnectionString(string configuredConnectionString, out bool useInMemory)
        {
            useInMemory = string.IsNullOrWhiteSpace(configuredConnectionString);
            return useInMemory 
                ? "DataSource=:memory:" 
                : configuredConnectionString;
        }
        
    }
}