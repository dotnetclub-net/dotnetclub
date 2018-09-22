using Microsoft.Extensions.DependencyInjection;
using System;
using Discussion.Migrations;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;


namespace Discussion.Web.ApplicationSupport
{
    public static class DatabaseMigrator
    {
        public static void Migrate(string connectionString, IConfiguration loggingConfiguration)
        {
            var services = CreateServices(connectionString, loggingConfiguration);

            using (var scope = services.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
            }
            
            (services as IDisposable)?.Dispose();
        }

        private static IServiceProvider CreateServices(string connectionString, IConfiguration loggingConfiguration)
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
                    Configurer.ConfigureFileLogging(logging, true, loggingConfiguration);
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