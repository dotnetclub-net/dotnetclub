using System;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Migrations.Supporting
{
    public static class SqliteMigrator
    {
        static void Main(string[] args)
        {
            var connectionString = args != null && args.Length > 0 ? args[0] : null;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                PrintError("Please specify a connection string for the Sqlite db.");
                Environment.Exit(1);
                return;
            }

            int? downToVersion = null;
            if (args.Length > 1 && int.TryParse(args[1], out var downTargetVersion))
            {
                downToVersion = downTargetVersion;
            }
            
            Console.WriteLine($"Starting migrating...");

            try
            {
                Migrate(connectionString, null, downToVersion);
            }
            catch (Exception e)
            {
                PrintError(e.ToString());
                Environment.Exit(4);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Migrating completed successfully");
            Console.ResetColor();
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }


        public static void Migrate(string connectionString, Action<ILoggingBuilder> migrationLogging, int? downToVersion = null)
        {
            var services = CreateServices(connectionString, migrationLogging);

            using (var scope = services.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider, downToVersion);
            }
            
            (services as IDisposable)?.Dispose();
        }

        private static IServiceProvider CreateServices(string connectionString, Action<ILoggingBuilder> configureLogging)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(CreateArticleTable).Assembly).For.Migrations())
                .AddLogging(logging =>
                {
                    logging.AddFluentMigratorConsole();
                    configureLogging?.Invoke(logging);
                })
                .BuildServiceProvider(false);
        }


        private static void UpdateDatabase(IServiceProvider serviceProvider, int? downToVersion)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            if (downToVersion.HasValue)
            {
                runner.MigrateDown(downToVersion.Value);
            }
            else
            {
                runner.MigrateUp();
            }
        }
    }
}