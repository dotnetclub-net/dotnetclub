using System;
using System.IO;
using Discussion.Migrations.Supporting;
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
        
        internal static void AddDataServices(this IServiceCollection services, IConfiguration appConfiguration, ILoggerFactory loggerFactory)
        {
            var connectionString = NormalizeSqliteConnectionString(appConfiguration[ConfigKey_ConnectionString], out var useTemporary);
            if (useTemporary)
            {
                loggerFactory.CreateLogger<Startup>().LogCritical("没有配置数据库连接字符串，将创建临时的数据库");
                appConfiguration[ConfigKey_ConnectionString] = connectionString;
            }
            
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
            var connectionString = NormalizeSqliteConnectionString(appConfiguration[ConfigKey_ConnectionString], out var useTemporary);

            var loggingConfigurtion = appConfiguration.GetSection(Configurer.ConfigKey_Logging);
            services.GetService<IApplicationLifetime>()
                .ApplicationStarted
                .Register(() => InitializeDatabase(connectionString, services.GetService<ILogger<Startup>>(), loggingConfigurtion));
        }

        static void InitializeDatabase(string connectionString, ILogger<Startup> logger, IConfiguration loggingConfiguration)
        {
            var sqliteConnection = new SqliteConnection(connectionString);
            var dataSource = sqliteConnection.DataSource;

            logger.LogInformation($"数据库位置：{dataSource}");
            if (!File.Exists(dataSource))
            {
                logger.LogCritical("正在创建新的数据库结构...");
                
                SqliteMigrator.Migrate(connectionString, logging => Configurer.ConfigureFileLogging(logging, true, loggingConfiguration));
                
                logger.LogCritical("数据库结构创建完成");
            }
        }

        static string NormalizeSqliteConnectionString(string configuredConnectionString, out bool useTemporary)
        {
            string randomDbName()
            {
                return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N").Substring(10, 8) + "-dnclub.db");
            }
            
            useTemporary = string.IsNullOrWhiteSpace(configuredConnectionString);
            return useTemporary 
                ? $"DataSource={randomDbName()}" 
                : configuredConnectionString;
        }
    }
}