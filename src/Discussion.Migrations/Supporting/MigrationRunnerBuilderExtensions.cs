using FluentMigrator.Runner;

namespace Discussion.Migrations.Supporting
{
    static class MigrationRunnerBuilderExtensions
    {
        internal static IMigrationRunnerBuilder AddDatabase(this IMigrationRunnerBuilder self, string connectionString)
        {
            if (connectionString.EndsWith("temp.db"))
                return self.AddSQLite().WithGlobalConnectionString(connectionString);

            return self.AddPostgres().WithGlobalConnectionString(connectionString);
        }
    }
}
