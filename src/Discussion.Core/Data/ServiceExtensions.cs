using System;
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
        public const string ConfigKeyConnectionString = "pgsqlConnectionString";
        public const string ConfigKeyIgnoreReadOnlySettings = "ignoreReadOnlySettings";

        public static void AddDataServices(this IServiceCollection services, IConfiguration appConfiguration, ILogger logger)
        {
            var connectionString = NormalizeConnectionString(appConfiguration[ConfigKeyConnectionString], out var createTemporary);
            if (createTemporary)
            {
                logger.LogCritical($"没有配置数据库连接字符串。可以用 \"{ConfigKeyConnectionString}\" 来配置数据库连接字符串。");
                appConfiguration[ConfigKeyConnectionString] = connectionString;
            }

            var useDatabase = PrepareDatabase(connectionString);
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                useDatabase(options);
            });
            services.AddScoped(typeof(IReadonlyDataSettings), typeof(ReadonlyDataSettings));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        }

        public static void EnsureDatabase(this IApplicationBuilder app, Action<string> databaseInitializer, ILogger logger)
        {
            var services = app.ApplicationServices;
            var appConfiguration = services.GetService<IConfiguration>();
            var connectionString = appConfiguration[ConfigKeyConnectionString];

            logger.LogInformation($"数据库位置：{connectionString}");

            services.GetService<IApplicationLifetime>()
                .ApplicationStarted
                .Register(() => databaseInitializer(connectionString));
        }
        
        public static bool IsForTemporaryDatabase(this string connectionString)
        {
            return connectionString.EndsWith("temp.db");
        }

        static string NormalizeConnectionString(string configuredConnectionString, out bool createTemporary)
        {
            createTemporary = string.IsNullOrWhiteSpace(configuredConnectionString);
            return createTemporary ? $"Data Source=dotnetclub-{StringUtility.Random(6)}-temp.db" : configuredConnectionString;
        }

        static Action<DbContextOptionsBuilder> PrepareDatabase(string connectionString)
        {
            // If use in-memory mode, then persist the db connection across ApplicationDbContext instances
            if (connectionString.IsForTemporaryDatabase())
                return options => options.UseSqlite(new SqliteConnection(connectionString));

            return options => options.UseNpgsql(connectionString);
        }
    }
}
