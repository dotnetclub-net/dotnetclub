
using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Data
{
    public static class ApplicationDataServices
    {
        private const string ConfigKeyConnectionString = "sqliteConnectionString";
        
        public static void AddDataServices(this IServiceCollection services, IConfiguration appConfiguration, ILogger logger)
        {
            var connectionString = NormalizeConnectionString(appConfiguration[ConfigKeyConnectionString], out var createTemporary);
            if (createTemporary)
            {
                logger.LogCritical($"没有配置数据库连接字符串。可以用 \"{ConfigKeyConnectionString}\" 来配置数据库连接字符串。");
                appConfiguration[ConfigKeyConnectionString] = connectionString;
            }
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        }
        
        public static void EnsureDatabase(this IApplicationBuilder app, Action<string> databaseInitializer, ILogger logger)
        {
            var services = app.ApplicationServices;
            var appConfiguration = services.GetService<IConfiguration>();

            var connectionString = appConfiguration[ConfigKeyConnectionString];
            var dataSource = new SqliteConnection(connectionString).DataSource;
            logger.LogInformation($"数据库位置：{dataSource}");
            
            if (!File.Exists(dataSource))
            {
                services.GetService<IApplicationLifetime>()
                    .ApplicationStarted
                    .Register(() => databaseInitializer(connectionString));     
            }
        }

        
        
        private static string NormalizeConnectionString(string configuredConnectionString, out bool createTemporary)
        {
            string RandomDbName()
            {
                return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N").Substring(10, 8) + "-dnclub.db");
            }
            
            createTemporary = string.IsNullOrWhiteSpace(configuredConnectionString);
            return createTemporary 
                ? $"Data Source={RandomDbName()}" 
                : configuredConnectionString;
        }
    }
}