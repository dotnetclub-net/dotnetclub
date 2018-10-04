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
            
            Console.WriteLine($"Starting migrating...");

            try
            {
                Migrate(connectionString, null);
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


        public static void Migrate(string connectionString, Action<ILoggingBuilder> migrationLogging)
        {
            var services = CreateServices(connectionString, migrationLogging);

            using (var scope = services.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
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


        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }
}