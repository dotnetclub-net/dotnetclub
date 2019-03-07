
using System;
using System.IO;
using Discussion.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Data
{
    public static class ServiceExtensions
    {
        public const string ConfigKeyConnectionString = "sqliteConnectionString";
        public const string ConfigKeyIgnoreReadOnlySettings = "ignoreReadOnlySettings";

        public static void AddDataServices(this IServiceCollection services, IConfiguration appConfiguration, ILogger logger)
        {
            var connectionString = NormalizeConnectionString(appConfiguration[ConfigKeyConnectionString], out var createTemporary);
            if (createTemporary)
            {
                logger.LogCritical($"没有配置数据库连接字符串。可以用 \"{ConfigKeyConnectionString}\" 来配置数据库连接字符串。");
                appConfiguration[ConfigKeyConnectionString] = connectionString;
            }

            var useSqlite = PrepareSqlite(connectionString);
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                useSqlite(options);
            });
            services.AddScoped(typeof(IReadonlyDataSettings), typeof(ReadonlyDataSettings));
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
                return Path.Combine(Path.GetTempPath(), $"{StringUtility.Random()}-dnclub.db");
            }

            createTemporary = string.IsNullOrWhiteSpace(configuredConnectionString);
            return createTemporary
                ? $"Data Source={RandomDbName()}"
                : configuredConnectionString;
        }

        private static Action<DbContextOptionsBuilder> PrepareSqlite(string connectionString)
        {
            // If use in-memory mode, then persist the db connection across ApplicationDbContext instances
            if (connectionString.Contains(":memory:"))
            {
                var connection = new SqliteConnection(connectionString);
                return options => options.UseSqlite(connection);
            }
            else
            {
                return options => options.UseSqlite(connectionString);
            }
        }
    }
}
